using Rin.Platform.Abstractions.Rendering;
using Rin.Rendering;
using Serilog;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using System.Drawing;
using AttachmentLoadOp = Silk.NET.Vulkan.AttachmentLoadOp;
using IWindow = Silk.NET.Windowing.IWindow;
using Semaphore = Silk.NET.Vulkan.Semaphore;

namespace Rin.Platform.Vulkan;

sealed class VulkanSwapChain : ISwapchain, IDisposable {
    readonly ILogger log = Log.ForContext<ISwapchain>();
    readonly Vk vk = VulkanContext.Vulkan;
    readonly Device vkDevice = VulkanContext.CurrentDevice.VkLogicalDevice;

    ColorSpaceKHR colorSpace;

    // Instances from Silk.NET
    KhrSurface vkSurface;
    KhrSwapchain vkSwapchain;

    SwapchainKHR? swapchain;
    SurfaceKHR surface;

    readonly List<SwapchainImage> images = new();
    readonly List<Fence> waitFences = new();

    int currentImageIndex;
    readonly List<Framebuffer> framebuffers = new();
    readonly List<SwapchainCommandBuffer> commandBuffers = new();
    SwapchainSemaphores semaphores;

    public Size Size { get; private set; }
    public bool VSync { get; set; }
    public int CurrentBufferIndex { get; private set; }
    public Format ColorFormat { get; private set; }
    public RenderPass? RenderPass { get; private set; }
    public Framebuffer CurrentFramebuffer => GetFrameBuffer(currentImageIndex);
    public CommandBuffer CurrentDrawCommandBuffer => GetDrawCommandBuffer(CurrentBufferIndex);
    public Semaphore? RenderCompleteSemaphore => semaphores.RenderComplete;
    public IEnumerable<Image> Images => images.Select(x => x.Image);

    public unsafe void InitializeSurface(IWindow window) {
        var instance = vk.CurrentInstance!.Value;

        if (!vk.TryGetInstanceExtension(instance, out vkSurface)) {
            throw new NotSupportedException("KHR_surface extension not found.");
        }

        surface = window.VkSurface!.Create<AllocationCallbacks>(instance.ToHandle(), null).ToSurface();

        FindImageFormatAndColorSpace();
        log.Debug("Surface initialized");
    }

    public void Create(ref Size size, bool vSync) {
        VSync = vSync;

        CreateSwapchain(ref size);
        CreateImageViews();
        CreateCommandBuffers();
        CreateSynchronizationObjects();
        CreateRenderPass();
        CreateFramebuffers();
    }

    public void OnResize(Size size) {
        vk.DeviceWaitIdle(vkDevice).EnsureSuccess();
        Create(ref size, VSync);
        vk.DeviceWaitIdle(vkDevice).EnsureSuccess();
    }

    public void BeginFrame() {
        log.Verbose("[SwapChain] Begin Frame");
        Renderer.GetRenderDisposeQueue(CurrentBufferIndex).Execute();

        currentImageIndex = AcquireNextImage();
        vk.ResetCommandPool(vkDevice, commandBuffers[CurrentBufferIndex].CommandPool, 0).EnsureSuccess();
    }

    public unsafe void Present() {
        log.Verbose("[SwapChain] Present");

        var waitStages = stackalloc[] { PipelineStageFlags.ColorAttachmentOutputBit };
        var waitSemaphores = stackalloc[] { semaphores.PresentComplete!.Value };
        var signalSemaphores = stackalloc[] { semaphores.RenderComplete!.Value };
        var commandBuffer = commandBuffers[CurrentBufferIndex].CommandBuffer;

        var submitInfo = new SubmitInfo(StructureType.SubmitInfo) {
            PWaitDstStageMask = waitStages,
            PWaitSemaphores = waitSemaphores,
            WaitSemaphoreCount = 1,
            PSignalSemaphores = signalSemaphores,
            SignalSemaphoreCount = 1,
            PCommandBuffers = &commandBuffer,
            CommandBufferCount = 1
        };

        var graphicsQueue = VulkanContext.CurrentDevice.GraphicsQueue;
        vk.ResetFences(vkDevice, 1, waitFences[CurrentBufferIndex]).EnsureSuccess();
        vk.QueueSubmit(graphicsQueue, 1, in submitInfo, waitFences[CurrentBufferIndex]).EnsureSuccess();

        var swapchain = this.swapchain!.Value;
        fixed (int* currentImageIndexPtr = &currentImageIndex) {
            var presentInfo = new PresentInfoKHR(StructureType.PresentInfoKhr) {
                PSwapchains = &swapchain,
                SwapchainCount = 1,
                PImageIndices = (uint*)currentImageIndexPtr,
                PWaitSemaphores = signalSemaphores,
                WaitSemaphoreCount = 1
            };

            var result = vkSwapchain.QueuePresent(graphicsQueue, presentInfo);
            if (result != Result.Success) {
                if (result is Result.ErrorOutOfDateKhr or Result.SuboptimalKhr) {
                    OnResize(Size);
                } else {
                    throw new("error rendering");
                }
            }
        }

        // TODO: performance timers
        CurrentBufferIndex = (CurrentBufferIndex + 1) % Renderer.Options.FramesInFlight;
        vk.WaitForFences(vkDevice, 1, waitFences[CurrentBufferIndex], true, uint.MaxValue).EnsureSuccess();
    }

    public Framebuffer GetFrameBuffer(int index) => framebuffers[index];
    public CommandBuffer GetDrawCommandBuffer(int index) => commandBuffers[index].CommandBuffer;

    public unsafe void Dispose() {
        vk.DeviceWaitIdle(vkDevice).EnsureSuccess();

        if (swapchain != null) {
            vkSwapchain.DestroySwapchain(vkDevice, swapchain.Value, null);
        }

        foreach (var image in images) {
            vk.DestroyImageView(vkDevice, image.ImageView, null);
        }

        foreach (var commandBuffer in commandBuffers) {
            vk.DestroyCommandPool(vkDevice, commandBuffer.CommandPool, null);
        }

        if (RenderPass.HasValue) {
            vk.DestroyRenderPass(vkDevice, RenderPass.Value, null);
        }

        foreach (var framebuffer in framebuffers) {
            vk.DestroyFramebuffer(vkDevice, framebuffer, null);
        }

        if (semaphores.RenderComplete != null) {
            vk.DestroySemaphore(vkDevice, semaphores.RenderComplete.Value, null);
        }

        if (semaphores.PresentComplete != null) {
            vk.DestroySemaphore(vkDevice, semaphores.PresentComplete.Value, null);
        }

        foreach (var fence in waitFences) {
            vk.DestroyFence(vkDevice, fence, null);
        }

        vkSurface.DestroySurface(VulkanContext.Vulkan.CurrentInstance!.Value, surface, null);
        vk.DeviceWaitIdle(vkDevice).EnsureSuccess();

        vkSurface.Dispose();
        vkSwapchain.Dispose();
    }

    unsafe void CreateSwapchain(ref Size size) {
        var physicalDevice = VulkanContext.CurrentDevice.PhysicalDevice.VkPhysicalDevice;

        var oldSwapchain = swapchain;
        vkSurface.GetPhysicalDeviceSurfaceCapabilities(physicalDevice, surface, out var surfCaps).EnsureSuccess();

        // Get available present modes
        uint presentModeCount = 0;
        vkSurface.GetPhysicalDeviceSurfacePresentModes(physicalDevice, surface, ref presentModeCount, null)
            .EnsureSuccess();

        using var handle = VulkanUtils.Alloc<PresentModeKHR>(presentModeCount, out var presentModes);
        vkSurface.GetPhysicalDeviceSurfacePresentModes(physicalDevice, surface, ref presentModeCount, presentModes)
            .EnsureSuccess();

        var swapchainExtent = new Extent2D();
        if (surfCaps.CurrentExtent.Width == uint.MaxValue) {
            swapchainExtent.Width = (uint)size.Width;
            swapchainExtent.Height = (uint)size.Height;
        } else {
            swapchainExtent = surfCaps.CurrentExtent;
            size.Width = (int)surfCaps.CurrentExtent.Width;
            size.Height = (int)surfCaps.CurrentExtent.Height;
        }

        Size = new(size.Width, size.Height);

        if (size.Width == 0 || size.Height == 0) {
            throw new("Width or height is set to 0");
        }

        var swapchainPresentMode = PresentModeKHR.FifoKhr;
        if (!VSync) {
            for (var i = 0; i < presentModeCount; i++) {
                if (presentModes[i] == PresentModeKHR.MailboxKhr) {
                    swapchainPresentMode = PresentModeKHR.MailboxKhr;
                    break;
                }

                if (presentModes[i] == PresentModeKHR.ImmediateKhr) {
                    swapchainPresentMode = PresentModeKHR.ImmediateKhr;
                }
            }
        }

        // Determine the number of images
        var desiredNumberOfSwapchainImages = surfCaps.MinImageCount + 1;
        if (surfCaps.MaxImageCount > 0 && desiredNumberOfSwapchainImages > surfCaps.MaxImageCount) {
            desiredNumberOfSwapchainImages = surfCaps.MaxImageCount;
        }

        // Transformation of the surface
        var preTransform = surfCaps.SupportedTransforms.HasFlag(SurfaceTransformFlagsKHR.IdentityBitKhr)
            ? SurfaceTransformFlagsKHR.IdentityBitKhr
            : surfCaps.CurrentTransform;

        // Find supported composite alpha format
        var compositeAlpha = CompositeAlphaFlagsKHR.OpaqueBitKhr;
        var compositeAlphaFlags = new[] {
            CompositeAlphaFlagsKHR.OpaqueBitKhr, CompositeAlphaFlagsKHR.PreMultipliedBitKhr,
            CompositeAlphaFlagsKHR.PostMultipliedBitKhr, CompositeAlphaFlagsKHR.InheritBitKhr
        };

        foreach (var flag in compositeAlphaFlags) {
            if (surfCaps.SupportedCompositeAlpha.HasFlag(flag)) {
                compositeAlpha = flag;
                break;
            }
        }

        var swapchainCreateInfo = new SwapchainCreateInfoKHR {
            SType = StructureType.SwapchainCreateInfoKhr,
            Surface = surface,
            MinImageCount = desiredNumberOfSwapchainImages,
            ImageFormat = ColorFormat,
            ImageColorSpace = colorSpace,
            ImageExtent = swapchainExtent,
            ImageUsage = ImageUsageFlags.ColorAttachmentBit,
            PreTransform = preTransform,
            ImageArrayLayers = 1,
            ImageSharingMode = SharingMode.Exclusive,
            QueueFamilyIndexCount = 0,
            PresentMode = swapchainPresentMode,
            OldSwapchain = oldSwapchain ?? default,
            Clipped = true,
            CompositeAlpha = compositeAlpha
        };

        if (surfCaps.SupportedUsageFlags.HasFlag(ImageUsageFlags.TransferSrcBit)) {
            swapchainCreateInfo.ImageUsage |= ImageUsageFlags.TransferSrcBit;
        }

        if (surfCaps.SupportedUsageFlags.HasFlag(ImageUsageFlags.TransferDstBit)) {
            swapchainCreateInfo.ImageUsage |= ImageUsageFlags.TransferDstBit;
        }

        if (!vk.TryGetDeviceExtension(vk.CurrentInstance!.Value, vkDevice, out vkSwapchain)) {
            throw new NotSupportedException("KHR_swapchain extension not found.");
        }

        vkSwapchain.CreateSwapchain(vkDevice, &swapchainCreateInfo, null, out var newSwapchain).EnsureSuccess();
        swapchain = newSwapchain;

        if (oldSwapchain != null) {
            vkSwapchain.DestroySwapchain(vkDevice, oldSwapchain.Value, null);
        }
    }

    unsafe void CreateImageViews() {
        foreach (var image in images) {
            vk.DestroyImage(vkDevice, image.Image, null);
        }
        images.Clear();

        // Get new swapchain images
        uint imagesCount = 0;
        vkSwapchain.GetSwapchainImages(vkDevice, swapchain!.Value, ref imagesCount, null).EnsureSuccess();
        using var handle = VulkanUtils.Alloc<Image>(imagesCount, out var swapchainImages);
        vkSwapchain.GetSwapchainImages(vkDevice, swapchain.Value, ref imagesCount, swapchainImages).EnsureSuccess();

        for (var i = 0; i < imagesCount; i++) {
            var imageViewCreateInfo = new ImageViewCreateInfo {
                SType = StructureType.ImageViewCreateInfo,
                Format = ColorFormat,
                Image = swapchainImages[i],
                Components = new(ComponentSwizzle.R, ComponentSwizzle.G, ComponentSwizzle.B, ComponentSwizzle.A),
                SubresourceRange = new(ImageAspectFlags.ColorBit, 0, 1, 0, 1),
                ViewType = ImageViewType.Type2D
            };

            vk.CreateImageView(vkDevice, imageViewCreateInfo, null, out var imageView).EnsureSuccess();
            VulkanUtils.SetDebugObjectName(ObjectType.ImageView, $"Swapchain ImageView: {i}", imageView.Handle);
            images.Add(new() { Image = swapchainImages[i], ImageView = imageView });
        }
    }

    unsafe void CreateCommandBuffers() {
        foreach (var commandBuffer in commandBuffers) {
            vk.DestroyCommandPool(vkDevice, commandBuffer.CommandPool, null);
        }
        commandBuffers.Clear();

        var cmdPoolInfo = new CommandPoolCreateInfo(StructureType.CommandPoolCreateInfo) {
            QueueFamilyIndex = VulkanContext.CurrentDevice.PhysicalDevice.QueueFamilyIndices.Graphics.Value,
            Flags = CommandPoolCreateFlags.TransientBit
        };

        var cmdAllocateInfo = new CommandBufferAllocateInfo(StructureType.CommandBufferAllocateInfo) {
            Level = CommandBufferLevel.Primary, CommandBufferCount = 1
        };

        for (var i = 0; i < images.Count; i++) {
            vk.CreateCommandPool(vkDevice, cmdPoolInfo, null, out var commandPool).EnsureSuccess();

            cmdAllocateInfo.CommandPool = commandPool;
            vk.AllocateCommandBuffers(vkDevice, cmdAllocateInfo, out var commandBuffer).EnsureSuccess();

            commandBuffers.Add(new() { CommandPool = commandPool, CommandBuffer = commandBuffer });
        }
    }

    unsafe void CreateSynchronizationObjects() {
        if (!semaphores.RenderComplete.HasValue || semaphores.PresentComplete.HasValue) {
            var semaphoreCreateInfo = new SemaphoreCreateInfo(StructureType.SemaphoreCreateInfo);

            vk.CreateSemaphore(vkDevice, semaphoreCreateInfo, null, out var renderComplete).EnsureSuccess();
            VulkanUtils.SetDebugObjectName(
                ObjectType.Semaphore,
                "Swapchain Semaphore RenderComplete",
                renderComplete.Handle
            );

            vk.CreateSemaphore(vkDevice, semaphoreCreateInfo, null, out var presentComplete).EnsureSuccess();
            VulkanUtils.SetDebugObjectName(
                ObjectType.Semaphore,
                "Swapchain Semaphore PresentComplete",
                presentComplete.Handle
            );

            semaphores = new() { RenderComplete = renderComplete, PresentComplete = presentComplete };
        }

        if (waitFences.Count != images.Count) {
            foreach (var fence in waitFences) {
                vk.DestroyFence(vkDevice, fence, null);
            }

            waitFences.Clear();
            var fenceCreateInfo = new FenceCreateInfo(StructureType.FenceCreateInfo) {
                Flags = FenceCreateFlags.SignaledBit
            };

            for (var i = 0; i < images.Count; i++) {
                vk.CreateFence(vkDevice, fenceCreateInfo, null, out var fence).EnsureSuccess();
                VulkanUtils.SetDebugObjectName(
                    ObjectType.Fence,
                    $"Swapchain Fence: {i}",
                    fence.Handle
                );
                waitFences.Add(fence);
            }
        }
    }

    unsafe void CreateRenderPass() {
        // TODO: not sure about this
        // if (RenderPass.HasValue) {
        //     vk.DestroyRenderPass(vkDevice, RenderPass.Value, null);
        // }
        
        var colorAttachment = new AttachmentDescription {
            Format = ColorFormat,
            Samples = SampleCountFlags.Count1Bit,
            LoadOp = AttachmentLoadOp.Clear,
            StoreOp = AttachmentStoreOp.Store,
            StencilLoadOp = AttachmentLoadOp.DontCare,
            StencilStoreOp = AttachmentStoreOp.DontCare,
            InitialLayout = ImageLayout.Undefined,
            FinalLayout = ImageLayout.PresentSrcKhr
        };

        var colorAttachmentRef = new AttachmentReference {
            Attachment = 0, Layout = ImageLayout.ColorAttachmentOptimal
        };

        var subpass = new SubpassDescription {
            PipelineBindPoint = PipelineBindPoint.Graphics,
            ColorAttachmentCount = 1,
            PColorAttachments = &colorAttachmentRef
        };

        var dependency = new SubpassDependency {
            SrcSubpass = Vk.SubpassExternal,
            DstSubpass = 0,
            SrcStageMask = PipelineStageFlags.ColorAttachmentOutputBit,
            SrcAccessMask = 0,
            DstStageMask = PipelineStageFlags.ColorAttachmentOutputBit,
            // DstAccessMask = AccessFlags.ColorAttachmentWriteBit
            // TODO ImGui
            DstAccessMask = AccessFlags.ColorAttachmentWriteBit | AccessFlags.ColorAttachmentReadBit
        };

        var renderPassInfo = new RenderPassCreateInfo(StructureType.RenderPassCreateInfo) {
            AttachmentCount = 1,
            PAttachments = &colorAttachment,
            SubpassCount = 1,
            PSubpasses = &subpass,
            DependencyCount = 1,
            PDependencies = &dependency
        };

        vk.CreateRenderPass(vkDevice, renderPassInfo, null, out var pass).EnsureSuccess();
        RenderPass = pass;

        VulkanUtils.SetDebugObjectName(
            ObjectType.RenderPass,
            "Swapchain RenderPass",
            pass.Handle
        );
    }

    unsafe void CreateFramebuffers() {
        foreach (var framebuffer in framebuffers) {
            vk.DestroyFramebuffer(vkDevice, framebuffer, null);
        }
        framebuffers.Clear();

        var framebufferCreateInfo = new FramebufferCreateInfo(StructureType.FramebufferCreateInfo) {
            RenderPass = RenderPass!.Value,
            AttachmentCount = 1,
            Width = (uint)Size.Width,
            Height = (uint)Size.Height,
            Layers = 1
        };

        for (var i = 0; i < images.Count; i++) {
            var imageView = images[i].ImageView;
            framebufferCreateInfo.PAttachments = &imageView;

            vk.CreateFramebuffer(vkDevice, framebufferCreateInfo, null, out var framebuffer).EnsureSuccess();
            VulkanUtils.SetDebugObjectName(
                ObjectType.Framebuffer,
                $"Swapchain Framebuffer [Frame in Flight: {i}]",
                framebuffer.Handle
            );

            framebuffers.Add(framebuffer);
        }
    }

    int AcquireNextImage() {
        if (swapchain == null) {
            throw new("Failed to acquire next image. Swapchain not created");
        }

        uint imageIndex = 0;
        vkSwapchain.AcquireNextImage(
            vkDevice,
            swapchain.Value,
            ulong.MaxValue,
            semaphores.PresentComplete!.Value,
            default,
            ref imageIndex
        );
        // TODO
            // .EnsureSuccess();

        // TODO: verify if this cast is correct
        return (int)imageIndex;
    }

    unsafe void FindImageFormatAndColorSpace() {
        var physicalDevice = VulkanContext.CurrentDevice.PhysicalDevice.VkPhysicalDevice;

        uint formatCount = 0;
        vkSurface.GetPhysicalDeviceSurfaceFormats(physicalDevice, surface, ref formatCount, null).EnsureSuccess();

        using var handle = VulkanUtils.Alloc<SurfaceFormatKHR>(formatCount, out var surfaceFormats);
        vkSurface.GetPhysicalDeviceSurfaceFormats(
                physicalDevice,
                surface,
                ref formatCount,
                surfaceFormats
            )
            .EnsureSuccess();

        // If the surface format list only includes one entry with VK_FORMAT_UNDEFINED,
        // there is no preferred format, so we assume VK_FORMAT_B8G8R8A8_UNORM
        if (formatCount == 1 && surfaceFormats[0].Format == Format.Undefined) {
            ColorFormat = Format.B8G8R8A8Unorm;
            colorSpace = surfaceFormats[0].ColorSpace;
        } else {
            var found = false;
            for (var i = 0; i < formatCount; i++) {
                var format = surfaceFormats[i];

                if (format.Format == Format.B8G8R8A8Unorm) {
                    ColorFormat = format.Format;
                    colorSpace = format.ColorSpace;
                    found = true;
                    break;
                }
            }

            if (!found) {
                ColorFormat = surfaceFormats[0].Format;
                colorSpace = surfaceFormats[0].ColorSpace;
            }
        }

        log.Debug("ColorFormat {Format} ColorSpace {Space}", ColorFormat, colorSpace);
    }


    struct SwapchainCommandBuffer {
        public CommandPool CommandPool { get; set; }
        public CommandBuffer CommandBuffer { get; set; }
    }

    struct SwapchainSemaphores {
        public Semaphore? PresentComplete { get; set; }
        public Semaphore? RenderComplete { get; set; }
    }

    struct SwapchainImage {
        public Image Image { get; set; }
        public ImageView ImageView { get; set; }
    }
}
