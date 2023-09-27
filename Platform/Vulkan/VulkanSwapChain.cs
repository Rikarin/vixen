using Rin.Core.Abstractions;
using Serilog;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Windowing;
using Semaphore = Silk.NET.Vulkan.Semaphore;

namespace Rin.Platform.Vulkan;

sealed class VulkanSwapChain : IDisposable {
    readonly ILogger logger = Log.ForContext<VulkanSwapChain>();
    readonly Vk vk;
    readonly Device vkDevice;

    uint? queueNodeIndex;
    ColorSpaceKHR colorSpace;

    // Instances from Silk.NET
    KhrSurface vkSurface;
    KhrSwapchain vkSwapchain;

    SwapchainKHR? swapchain;
    SurfaceKHR surface;
    RenderPass? renderPass;

    readonly List<SwapchainImage> images = new();
    readonly List<Fence> waitFences = new();

    int currentImageIndex;
    readonly List<Framebuffer> framebuffers = new();
    readonly List<SwapchainCommandBuffer> commandBuffers = new();
    SwapchainSemaphores semaphores;


    // Was uint_32
    public int Width { get; private set; }

    public int Height { get; private set; }
    // public int ImageCount { get; private set; }

    public bool VSync { get; set; }
    public int CurrentBufferIndex { get; private set; }

    // public RenderPass VkRenderPass { get; private set; }
    public Format ColorFormat { get; private set; }

    public Framebuffer CurrentFramebuffer => GetFrameBuffer(currentImageIndex);
    public CommandBuffer CurrentDrawCommandBuffer => GetDrawCommandBuffer(CurrentBufferIndex);
    public Semaphore? RenderCompleteSemaphore => semaphores.RenderComplete;

    public VulkanSwapChain() {
        vk = VulkanContext.Vulkan;
        vkDevice = VulkanContext.CurrentDevice.VkLogicalDevice;
    }

    public unsafe void InitializeSurface(IWindow window) {
        var instance = vk.CurrentInstance!.Value;

        if (!vk.TryGetInstanceExtension(instance, out vkSurface)) {
            throw new NotSupportedException("KHR_surface extension not found.");
        }

        surface = window.VkSurface!.Create<AllocationCallbacks>(instance.ToHandle(), null).ToSurface();

        var physicalDevice = VulkanContext.CurrentDevice.PhysicalDevice;
        var queueProps = physicalDevice.QueueFamilyProperties;
        var supportsPresent = new bool[queueProps.Count];

        for (uint i = 0; i < queueProps.Count; i++) {
            vkSurface.GetPhysicalDeviceSurfaceSupport(physicalDevice.VkPhysicalDevice, i, surface, out var supported);
            supportsPresent[i] = supported;
        }

        uint? graphicsQueueNodeIndex = null;
        uint? presentQueueNodeIndex = null;
        for (var i = 0; i < queueProps.Count; i++) {
            if (queueProps[i].QueueFlags.HasFlag(QueueFlags.GraphicsBit)) {
                graphicsQueueNodeIndex ??= (uint)i;

                if (supportsPresent[i]) {
                    graphicsQueueNodeIndex = (uint)i;
                    presentQueueNodeIndex = (uint)i;
                    break;
                }
            }
        }

        if (presentQueueNodeIndex == null) {
            for (var i = 0; i < queueProps.Count; i++) {
                if (supportsPresent[i]) {
                    presentQueueNodeIndex = (uint)i;
                    break;
                }
            }
        }

        if (graphicsQueueNodeIndex == null || presentQueueNodeIndex == null) {
            throw new("Failed to find graphics or present queue index");
        }

        queueNodeIndex = graphicsQueueNodeIndex;
        FindImageFormatAndColorSpace();
        Log.Information("Surface initialized");
    }

    public void Create(ref int width, ref int height, bool vSync) {
        VSync = vSync;

        CreateSwapchain(ref width, ref height);
        CreateImageViews();
        CreateCommandBuffers();
        CreateSynchronizationObjects();
        CreateRenderPass();
        CreateFramebuffers();
    }

    // TODO: Not sure if this needs to has also ref params or not
    public void OnResize(int width, int height) {
        logger.Verbose("OnResize");
        var device = VulkanContext.CurrentDevice.VkLogicalDevice;

        vk.DeviceWaitIdle(device);
        Create(ref width, ref height, VSync);
        vk.DeviceWaitIdle(device);
    }

    public void BeginFrame() {
        Renderer.GetRenderResourceReleaseQueue(CurrentBufferIndex).Execute();

        currentImageIndex = AcquireNextImage();
        vk.ResetCommandPool(vkDevice, commandBuffers[CurrentBufferIndex].CommandPool, 0);
    }

    public unsafe void Present() {
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
        vk.ResetFences(vkDevice, 1, waitFences[CurrentBufferIndex]);
        vk.QueueSubmit(graphicsQueue, 1, in submitInfo, waitFences[CurrentBufferIndex]);

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
                    OnResize(Width, Height);
                } else {
                    throw new("error rendering");
                }
            }
        }

        // TODO: performance timers
        CurrentBufferIndex = (CurrentBufferIndex + 1) % 3; // TODO: take this from config
        vk.WaitForFences(vkDevice, 1, waitFences[CurrentBufferIndex], true, uint.MaxValue);
    }

    public Framebuffer GetFrameBuffer(int index) => framebuffers[index];
    public CommandBuffer GetDrawCommandBuffer(int index) => commandBuffers[index].CommandBuffer;

    public unsafe void Dispose() {
        vk.DeviceWaitIdle(vkDevice);

        if (swapchain != null) {
            vkSwapchain.DestroySwapchain(vkDevice, swapchain.Value, null);
        }

        foreach (var image in images) {
            vk.DestroyImageView(vkDevice, image.ImageView, null);
        }

        foreach (var commandBuffer in commandBuffers) {
            vk.DestroyCommandPool(vkDevice, commandBuffer.CommandPool, null);
        }

        if (renderPass.HasValue) {
            vk.DestroyRenderPass(vkDevice, renderPass.Value, null);
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
        vk.DeviceWaitIdle(vkDevice);

        vkSurface.Dispose();
        vkSwapchain.Dispose();
    }

    unsafe void CreateSwapchain(ref int width, ref int height) {
        var physicalDevice = VulkanContext.CurrentDevice.PhysicalDevice.VkPhysicalDevice;

        var oldSwapchain = swapchain;
        vkSurface.GetPhysicalDeviceSurfaceCapabilities(physicalDevice, surface, out var surfCaps);

        // Get available present modes
        uint presentModeCount = 0;
        vkSurface.GetPhysicalDeviceSurfacePresentModes(physicalDevice, surface, ref presentModeCount, null);

        using var handle = VulkanUtils.Alloc<PresentModeKHR>(presentModeCount, out var presentModes);
        vkSurface.GetPhysicalDeviceSurfacePresentModes(physicalDevice, surface, ref presentModeCount, presentModes);

        var swapchainExtent = new Extent2D();
        if (surfCaps.CurrentExtent.Width == uint.MaxValue) {
            swapchainExtent.Width = (uint)width;
            swapchainExtent.Height = (uint)height;
        } else {
            swapchainExtent = surfCaps.CurrentExtent;
            width = (int)surfCaps.CurrentExtent.Width;
            height = (int)surfCaps.CurrentExtent.Height;
        }

        Width = width;
        Height = height;

        if (width == 0 || height == 0) {
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

        vkSwapchain.CreateSwapchain(vkDevice, &swapchainCreateInfo, null, out var newSwapchain);
        swapchain = newSwapchain;

        if (oldSwapchain != null) {
            vkSwapchain.DestroySwapchain(vkDevice, oldSwapchain.Value, null);
        }
    }

    unsafe void CreateImageViews() {
        foreach (var image in images) {
            vk.DestroyImage(vkDevice, image.Image, null);
        }

        // Get new swapchain images
        uint imagesCount = 0;
        vkSwapchain.GetSwapchainImages(vkDevice, swapchain!.Value, ref imagesCount, null);
        using var handle = VulkanUtils.Alloc<Image>(imagesCount, out var swapchainImages);
        vkSwapchain.GetSwapchainImages(vkDevice, swapchain.Value, ref imagesCount, swapchainImages);

        Log.Information("Frames in flight: {Variable}", imagesCount);
        images.Clear();
        for (var i = 0; i < imagesCount; i++) {
            var imageViewCreateInfo = new ImageViewCreateInfo {
                SType = StructureType.ImageViewCreateInfo,
                Format = ColorFormat,
                Image = swapchainImages[i],
                Components = new(ComponentSwizzle.R, ComponentSwizzle.G, ComponentSwizzle.B, ComponentSwizzle.A),
                SubresourceRange = new(ImageAspectFlags.ColorBit, 0, 1, 0, 1),
                ViewType = ImageViewType.Type2D,
                Flags = 0
            };

            vk.CreateImageView(vkDevice, imageViewCreateInfo, null, out var imageView);
            VulkanUtils.SetDebugObjectName(ObjectType.ImageView, $"Swapchain ImageView: {i}", imageView.Handle);
            images.Add(new() { Image = swapchainImages[i], ImageView = imageView });
        }
    }

    unsafe void CreateCommandBuffers() {
        foreach (var commandBuffer in commandBuffers) {
            vk.DestroyCommandPool(vkDevice, commandBuffer.CommandPool, null);
        }

        var cmdPoolInfo = new CommandPoolCreateInfo(StructureType.CommandPoolCreateInfo) {
            QueueFamilyIndex = queueNodeIndex!.Value, Flags = CommandPoolCreateFlags.TransientBit
        };

        var cmdAllocateInfo = new CommandBufferAllocateInfo(StructureType.CommandBufferAllocateInfo) {
            Level = CommandBufferLevel.Primary, CommandBufferCount = 1
        };

        commandBuffers.Clear();
        for (var i = 0; i < images.Count; i++) {
            vk.CreateCommandPool(vkDevice, cmdPoolInfo, null, out var commandPool);

            cmdAllocateInfo.CommandPool = commandPool;
            vk.AllocateCommandBuffers(vkDevice, cmdAllocateInfo, out var commandBuffer);

            commandBuffers.Add(new() { CommandPool = commandPool, CommandBuffer = commandBuffer });
        }
    }

    unsafe void CreateSynchronizationObjects() {
        if (!semaphores.RenderComplete.HasValue || semaphores.PresentComplete.HasValue) {
            var semaphoreCreateInfo = new SemaphoreCreateInfo(StructureType.SemaphoreCreateInfo);

            vk.CreateSemaphore(vkDevice, semaphoreCreateInfo, null, out var renderComplete);
            VulkanUtils.SetDebugObjectName(
                ObjectType.Semaphore,
                "Swapchain Semaphore RenderComplete",
                renderComplete.Handle
            );

            vk.CreateSemaphore(vkDevice, semaphoreCreateInfo, null, out var presentComplete);
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
                vk.CreateFence(vkDevice, fenceCreateInfo, null, out var fence);
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
            DstAccessMask = AccessFlags.ColorAttachmentWriteBit
        };

        var renderPassInfo = new RenderPassCreateInfo(StructureType.RenderPassCreateInfo) {
            AttachmentCount = 1,
            PAttachments = &colorAttachment,
            SubpassCount = 1,
            PSubpasses = &subpass,
            DependencyCount = 1,
            PDependencies = &dependency
        };

        vk.CreateRenderPass(vkDevice, renderPassInfo, null, out var pass);
        renderPass = pass;
        
        VulkanUtils.SetDebugObjectName(
            ObjectType.RenderPass,
            $"Swapchain RenderPass",
            pass.Handle
        );
    }

    unsafe void CreateFramebuffers() {
        foreach (var framebuffer in framebuffers) {
            vk.DestroyFramebuffer(vkDevice, framebuffer, null);
        }

        var framebufferCreateInfo = new FramebufferCreateInfo(StructureType.FramebufferCreateInfo) {
            RenderPass = renderPass!.Value,
            AttachmentCount = 1,
            Width = (uint)Width,
            Height = (uint)Height,
            Layers = 1
        };

        for (var i = 0; i < images.Count; i++) {
            var imageView = images[i].ImageView;
            framebufferCreateInfo.PAttachments = &imageView;
            
            var result = vk.CreateFramebuffer(vkDevice, framebufferCreateInfo, null, out var framebuffer);
            if (result != Result.Success) {
                Log.Fatal("Failed to create framebuffer");
            }
            
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
            VulkanContext.CurrentDevice.VkLogicalDevice,
            swapchain.Value,
            ulong.MaxValue,
            semaphores.PresentComplete!.Value,
            default,
            ref imageIndex
        );

        // TODO: verify if this cast is correct
        return (int)imageIndex;
    }

    unsafe void FindImageFormatAndColorSpace() {
        var physicalDevice = VulkanContext.CurrentDevice.PhysicalDevice.VkPhysicalDevice;

        uint formatCount = 0;
        vkSurface.GetPhysicalDeviceSurfaceFormats(physicalDevice, surface, ref formatCount, null);

        Log.Information("Debug: {Variable}", formatCount);
        using var handle = VulkanUtils.Alloc<SurfaceFormatKHR>(formatCount, out var surfaceFormats);
        vkSurface.GetPhysicalDeviceSurfaceFormats(
            physicalDevice,
            surface,
            ref formatCount,
            surfaceFormats
        );

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

        Log.Information("ColorFormat {Format} ColorSpace {Space}", ColorFormat, colorSpace);
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
