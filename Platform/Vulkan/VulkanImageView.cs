using Rin.Core.Abstractions;
using Rin.Platform.Rendering;
using Silk.NET.Vulkan;

namespace Rin.Platform.Vulkan;

public class VulkanImageView : IImageView, IDisposable {
    readonly ImageViewOptions options;
    ImageView imageView;

    public DescriptorImageInfo DescriptorImageInfo { get; private set; }

    public VulkanImageView(ImageViewOptions options) {
        this.options = options;
        Invalidate();
    }

    public unsafe void Dispose() {
        Renderer.SubmitDisposal(
            () => {
                VulkanContext.Vulkan.DestroyImageView(VulkanContext.CurrentDevice.VkLogicalDevice, imageView, null);
            }
        );
    }

    void Invalidate() => Renderer.Submit(Invalidate_RT);

    unsafe void Invalidate_RT() {
        var device = VulkanContext.CurrentDevice.VkLogicalDevice;
        var image = options.Image as VulkanImage2D;
        var imageOptions = image!.Options;

        var imageViewCreateInfo = new ImageViewCreateInfo(StructureType.ImageViewCreateInfo) {
            ViewType = imageOptions.Layers > 1 ? ImageViewType.Type2DArray : ImageViewType.Type2D,
            Format = imageOptions.Format.ToVulkanImageFormat(),
            Image = image.ImageInfo.Image!.Value,
            SubresourceRange = new() {
                AspectMask = image.ImageAspectMask,
                BaseMipLevel = (uint)options.Mip,
                LevelCount = 1,
                LayerCount = (uint)imageOptions.Layers
            }
        };

        VulkanContext.Vulkan.CreateImageView(device, imageViewCreateInfo, null, out var imageView);
        VulkanUtils.SetDebugObjectName(
            ObjectType.ImageView,
            $"{options.DebugName} default image view",
            imageView.Handle
        );

        this.imageView = imageView;
        DescriptorImageInfo = image.DescriptorImageInfo with { ImageView = imageView };
    }
}
