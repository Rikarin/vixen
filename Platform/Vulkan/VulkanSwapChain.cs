using Serilog;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Windowing;
using System.Runtime.CompilerServices;
using Semaphore = Silk.NET.Vulkan.Semaphore;

namespace Rin.Platform.Vulkan;

sealed class VulkanSwapChain : IDisposable {
    // readonly Instance instance;
    // readonly VulkanDevice device;
    readonly ILogger logger = Log.ForContext<VulkanSwapChain>();

    uint? queueNodeIndex;
    ColorSpaceKHR colorSpace;

    // Instances from Silk.NET
    KhrSurface vkSurface;
    KhrSwapchain vkSwapchain;

    SwapchainKHR? swapchain;
    SurfaceKHR surface;

    int currentImageIndex;
    readonly List<Framebuffer> framebuffers = new();
    readonly List<SwapchainCommandBuffer> commandBuffers = new();
    readonly SwapchainSemaphores semaphores;


    // Was uint_32
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int ImageCount { get; private set; }

    public bool VSync { get; set; }
    public int CurrentBufferIndex { get; private set; }

    public RenderPass VkRenderPass { get; private set; }
    public Format ColorFormat { get; private set; }

    public Framebuffer CurrentFramebuffer => GetFrameBuffer(currentImageIndex);
    public CommandBuffer CurrentDrawCommandBuffer => GetDrawCommandBuffer(CurrentBufferIndex);
    public Semaphore? RenderCompleteSemaphore => semaphores.RenderComplete;

    public unsafe void InitializeSurface(IWindow window) {
        var vk = VulkanContext.Vulkan;
        var instance = vk.CurrentInstance!.Value;

        if (!vk.TryGetInstanceExtension(instance, out vkSurface)) {
            throw new NotSupportedException("KHR_surface extension not found.");
        }

        surface = window.VkSurface!.Create<AllocationCallbacks>(instance.ToHandle(), null).ToSurface();

        var device = VulkanContext.CurrentDevice;
        var physicalDevice = device.PhysicalDevice;
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

    public unsafe void Create(ref int width, ref int height, bool vSync) {
        VSync = vSync;

        var device = VulkanContext.CurrentDevice;
        var physicalDevice = device.PhysicalDevice.VkPhysicalDevice;

        var oldSwapchain = swapchain;
        vkSurface.GetPhysicalDeviceSurfaceCapabilities(physicalDevice, surface, out var surfCaps);

        // Get available present modes
        uint presentModeCount = 0;
        vkSurface.GetPhysicalDeviceSurfacePresentModes(physicalDevice, surface, ref presentModeCount, null);
        using var mem = GlobalMemory.Allocate((int)presentModeCount * sizeof(PresentModeKHR));
        var presentModes = (PresentModeKHR*)Unsafe.AsPointer(ref mem.GetPinnableReference());
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

        // Present modes
        var swapchainPresentMode = PresentModeKHR.FifoKhr;
        if (!vSync) {
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
        
        // Create swapchain
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

        var vk = VulkanContext.Vulkan;
        if (!vk.TryGetDeviceExtension(vk.CurrentInstance!.Value, device.VkLogicalDevice, out vkSwapchain)) {
            throw new NotSupportedException("KHR_swapchain extension not found.");
        }
        
        vkSwapchain.CreateSwapchain(device.VkLogicalDevice, &swapchainCreateInfo, null, out var newSwapchain);
        swapchain = newSwapchain;

        if (oldSwapchain != null) {
            vkSwapchain.DestroySwapchain(device.VkLogicalDevice, oldSwapchain.Value, null);
        }
        
        // TODO: images


        
        // TODO: finish this













    }

    // TODO: Not sure if this needs to has also ref params or not
    public void OnResize(int width, int height) {
        logger.Verbose("OnResize");

        var vk = VulkanContext.Vulkan;
        var device = VulkanContext.CurrentDevice;

        vk.DeviceWaitIdle(device.VkLogicalDevice);
        Create(ref width, ref height, VSync);
        vk.DeviceWaitIdle(device.VkLogicalDevice);
    }

    public void BeginFrame() {
        // TODO

        currentImageIndex = AcquireNextImage();
        VulkanContext.Vulkan.ResetCommandPool(
            VulkanContext.CurrentDevice.VkLogicalDevice,
            commandBuffers[currentImageIndex].CommandPool,
            0
        );
    }

    public void Present() {
        throw new NotImplementedException();
    }


    public Framebuffer GetFrameBuffer(int index) => framebuffers[index];
    public CommandBuffer GetDrawCommandBuffer(int index) => commandBuffers[index].CommandBuffer;

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
        var physicalDevice = VulkanContext.CurrentDevice.PhysicalDevice;

        uint formatCount = 0;
        vkSurface.GetPhysicalDeviceSurfaceFormats(physicalDevice.VkPhysicalDevice, surface, ref formatCount, null);

        Log.Information("Debug: {Variable}", formatCount);
        using var mem = GlobalMemory.Allocate((int)formatCount * sizeof(SurfaceFormatKHR));
        var surfaceFormats = (SurfaceFormatKHR*)Unsafe.AsPointer(ref mem.GetPinnableReference());
        vkSurface.GetPhysicalDeviceSurfaceFormats(
            physicalDevice.VkPhysicalDevice,
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

    public unsafe void Dispose() {
        var vk = VulkanContext.Vulkan;
        var device = VulkanContext.CurrentDevice.VkLogicalDevice;

        vk.DeviceWaitIdle(device);

        if (swapchain != null) {
            vkSwapchain.DestroySwapchain(device, swapchain.Value, null);
        }

        // TODO: images

        foreach (var commandBuffer in commandBuffers) {
            vk.DestroyCommandPool(device, commandBuffer.CommandPool, null);
        }

        // TODO render pass

        // framebuffers

        if (semaphores.RenderComplete != null) {
            vk.DestroySemaphore(device, semaphores.RenderComplete.Value, null);
        }

        if (semaphores.PresentComplete != null) {
            vk.DestroySemaphore(device, semaphores.PresentComplete.Value, null);
        }
        
        // TODO: fences
        // foreach (var fence in fences)




        vkSurface.DestroySurface(VulkanContext.Vulkan.CurrentInstance!.Value, surface, null);
        vk.DeviceWaitIdle(device);
        
        vkSurface.Dispose();
        vkSwapchain.Dispose();
    }
}
