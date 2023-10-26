using Rin.Core.Abstractions;
using Rin.Platform.Abstractions.Rendering;
using Rin.Platform.Vulkan.Allocator;
using Rin.Rendering;
using Silk.NET.Vulkan;

namespace Rin.Platform.Vulkan;

sealed class VulkanImage2D : IImage2D, IDisposable {
    readonly List<ImageView> perLayerImageViews = new();
    readonly Dictionary<int, ImageView> perMipImageViews = new();

    DescriptorImageInfo descriptorImageInfo;

    static readonly Dictionary<Image, VulkanImage2D> imageReferences = new();

    public DescriptorImageInfo DescriptorImageInfo => descriptorImageInfo;

    public ImageOptions Options { get; }
    public VulkanImageInfo ImageInfo { get; } = new();

    public ImageAspectFlags ImageAspectMask {
        get {
            var aspectMask = Options.Format.IsDepthFormat() ? ImageAspectFlags.DepthBit : ImageAspectFlags.ColorBit;
            if (Options.Format == ImageFormat.Depth24Stencil8) {
                aspectMask |= ImageAspectFlags.StencilBit;
            }

            return aspectMask;
        }
    }

    public VulkanImage2D(ImageOptions options) {
        Options = options;
        if (options.Size.Width < 1 || options.Size.Height < 1) {
            throw new ArgumentOutOfRangeException(nameof(options.Size));
        }
    }

    public ImageView GetLayerImageView(int layer) => perLayerImageViews[layer];

    public void Dispose() {
        Release();
    }

    public void Invalidate() => Renderer.Submit(Invalidate_RT);
    public void CreatePerLayerImageViews() => Renderer.Submit(CreatePerLayerImageViews_RT);

    public unsafe void Release() {
        if (ImageInfo.Image == null) {
            return;
        }

        Renderer.SubmitDisposal(
            () => {
                var device = VulkanContext.CurrentDevice.VkLogicalDevice;
                var vk = VulkanContext.Vulkan;

                vk.DestroyImageView(device, ImageInfo.ImageView!.Value, null);
                if (ImageInfo.Sampler.HasValue) {
                    VulkanSampler.DestroySampler(ImageInfo.Sampler.Value);
                }

                foreach (var view in perMipImageViews) {
                    vk.DestroyImageView(device, view.Value, null);
                }

                foreach (var view in perLayerImageViews) {
                    vk.DestroyImageView(device, view, null);
                }

                VulkanAllocator.DestroyImage(ImageInfo.Image.Value, ImageInfo.MemoryAllocation);
                imageReferences.Remove(ImageInfo.Image.Value);

                ImageInfo.Image = null;
                ImageInfo.ImageView = null;

                if (Options.CreateSampler) {
                    ImageInfo.Sampler = null;
                }

                perLayerImageViews.Clear();
                perMipImageViews.Clear();
            }
        );
    }

    public unsafe void CreatePerSpecificLayerImageViews_RT(IEnumerable<int> layerIndices) {
        var device = VulkanContext.CurrentDevice.VkLogicalDevice;

        foreach (var layer in layerIndices) {
            var imageViewCreateInfo = new ImageViewCreateInfo(StructureType.ImageViewCreateInfo) {
                ViewType = ImageViewType.Type2D,
                Format = Options.Format.ToVulkanImageFormat(),
                Image = ImageInfo.Image!.Value,
                SubresourceRange = new() {
                    AspectMask = ImageAspectMask,
                    LevelCount = (uint)Options.Mips,
                    BaseArrayLayer = (uint)layer,
                    LayerCount = 1
                }
            };

            VulkanContext.Vulkan.CreateImageView(device, imageViewCreateInfo, null, out var imageView).EnsureSuccess();
            VulkanUtils.SetDebugObjectName(
                ObjectType.ImageView,
                $"{Options.DebugName} image view layer: {layer}",
                imageView.Handle
            );

            // TODO: we need to use dictionary instead of list because this will fail
            perLayerImageViews[layer] = imageView;
        }
    }

    public unsafe void Invalidate_RT() {
        Release();

        var device = VulkanContext.CurrentDevice.VkLogicalDevice;
        var usage = ImageUsageFlags.SampledBit;

        if (Options.Usage == ImageUsage.Attachment) {
            usage |= Options.Format.IsDepthFormat()
                ? ImageUsageFlags.DepthStencilAttachmentBit
                : ImageUsageFlags.ColorAttachmentBit;
        }

        if (Options.Transfer || Options.Usage == ImageUsage.Texture) {
            usage |= ImageUsageFlags.TransferSrcBit | ImageUsageFlags.TransferDstBit;
        }

        if (Options.Usage == ImageUsage.Storage) {
            usage |= ImageUsageFlags.StorageBit | ImageUsageFlags.TransferDstBit;
        }

        var vkFormat = Options.Format.ToVulkanImageFormat();
        var memoryUsage = Options.Usage == ImageUsage.HostRead ? MemoryUsage.GPU_To_CPU : MemoryUsage.GPU_Only;

        var imageCreateInfo = new ImageCreateInfo(StructureType.ImageCreateInfo) {
            ImageType = ImageType.Type2D,
            Format = vkFormat,
            Extent = new() { Width = (uint)Options.Size.Width, Height = (uint)Options.Size.Height, Depth = 1 },
            MipLevels = (uint)Options.Mips,
            ArrayLayers = (uint)Options.Layers,
            Samples = SampleCountFlags.Count1Bit,
            Tiling = Options.Usage == ImageUsage.HostRead ? ImageTiling.Linear : ImageTiling.Optimal,
            Usage = usage
        };

        ImageInfo.MemoryAllocation = VulkanAllocator.AllocateImage(imageCreateInfo, memoryUsage, out var image);
        ImageInfo.Image = image;
        imageReferences[image] = this;
        VulkanUtils.SetDebugObjectName(ObjectType.Image, Options.DebugName, image.Handle);

        // Image View
        var imageViewCreateInfo = new ImageViewCreateInfo(StructureType.ImageViewCreateInfo) {
            ViewType = Options.Layers > 1 ? ImageViewType.Type2DArray : ImageViewType.Type2D,
            Format = vkFormat,
            SubresourceRange = new() {
                AspectMask = ImageAspectMask, LevelCount = (uint)Options.Mips, LayerCount = (uint)Options.Layers
            },
            Image = image
        };

        VulkanContext.Vulkan.CreateImageView(device, imageViewCreateInfo, null, out var imageView).EnsureSuccess();
        ImageInfo.ImageView = imageView;
        VulkanUtils.SetDebugObjectName(
            ObjectType.ImageView,
            $"{Options.DebugName} default image view",
            imageView.Handle
        );

        // Create Sampler
        if (Options.CreateSampler) {
            var samplerCreateInfo = new SamplerCreateInfo(StructureType.SamplerCreateInfo) {
                MaxAnisotropy = 1,
                AddressModeU = SamplerAddressMode.ClampToEdge,
                AddressModeV = SamplerAddressMode.ClampToEdge,
                AddressModeW = SamplerAddressMode.ClampToEdge,
                MaxLod = 100,
                BorderColor = BorderColor.FloatOpaqueWhite
            };

            if (Options.Format.IsIntegerBased()) {
                samplerCreateInfo.MagFilter = Filter.Nearest;
                samplerCreateInfo.MinFilter = Filter.Nearest;
                samplerCreateInfo.MipmapMode = SamplerMipmapMode.Nearest;
            } else {
                samplerCreateInfo.MagFilter = Filter.Linear;
                samplerCreateInfo.MinFilter = Filter.Linear;
                samplerCreateInfo.MipmapMode = SamplerMipmapMode.Linear;
            }

            ImageInfo.Sampler = VulkanSampler.CreateSampler(samplerCreateInfo);
            VulkanUtils.SetDebugObjectName(
                ObjectType.Sampler,
                $"{Options.DebugName} default sampler",
                ImageInfo.Sampler.Value.Handle
            );
        }

        if (Options.Usage == ImageUsage.Storage) {
            var commandBuffer = VulkanContext.CurrentDevice.GetCommandBuffer(true);
            var subresourceRange = new ImageSubresourceRange {
                AspectMask = ImageAspectFlags.ColorBit,
                LevelCount = (uint)Options.Mips,
                LayerCount = (uint)Options.Layers
            };

            VulkanUtils.InsertImageMemoryBarrier(
                commandBuffer,
                ImageInfo.Image.Value,
                AccessFlags.None,
                AccessFlags.None,
                ImageLayout.Undefined,
                ImageLayout.General,
                PipelineStageFlags.AllCommandsBit,
                PipelineStageFlags.AllCommandsBit,
                subresourceRange
            );

            VulkanContext.CurrentDevice.FlushCommandBuffer(commandBuffer);
        } else if (Options.Usage == ImageUsage.HostRead) {
            var commandBuffer = VulkanContext.CurrentDevice.GetCommandBuffer(true);
            var subresourceRange = new ImageSubresourceRange {
                AspectMask = ImageAspectFlags.ColorBit,
                LevelCount = (uint)Options.Mips,
                LayerCount = (uint)Options.Layers
            };

            VulkanUtils.InsertImageMemoryBarrier(
                commandBuffer,
                ImageInfo.Image.Value,
                AccessFlags.None,
                AccessFlags.None,
                ImageLayout.Undefined,
                ImageLayout.TransferDstOptimal,
                PipelineStageFlags.AllCommandsBit,
                PipelineStageFlags.AllCommandsBit,
                subresourceRange
            );

            VulkanContext.CurrentDevice.FlushCommandBuffer(commandBuffer);
        }

        UpdateDescriptor();
    }


    unsafe void CreatePerLayerImageViews_RT() {
        var device = VulkanContext.CurrentDevice.VkLogicalDevice;

        for (var layer = 0; layer < Options.Layers; layer++) {
            var imageViewCreateInfo = new ImageViewCreateInfo(StructureType.ImageViewCreateInfo) {
                ViewType = ImageViewType.Type2D,
                Format = Options.Format.ToVulkanImageFormat(),
                Image = ImageInfo.Image!.Value,
                SubresourceRange = new() {
                    AspectMask = ImageAspectMask,
                    LevelCount = (uint)Options.Mips,
                    BaseArrayLayer = (uint)layer,
                    LayerCount = 1
                }
            };

            VulkanContext.Vulkan.CreateImageView(device, imageViewCreateInfo, null, out var imageView).EnsureSuccess();
            VulkanUtils.SetDebugObjectName(
                ObjectType.ImageView,
                $"{Options.DebugName} image view layer: {layer}",
                imageView.Handle
            );
            perLayerImageViews[layer] = imageView;
        }
    }

    ImageView? GetMipImageView(int mip) {
        if (perMipImageViews.TryGetValue(mip, out var value)) {
            return value;
        }

        Renderer.Submit(() => GetMipImageView_RT(mip));
        return null;
    }

    unsafe ImageView GetMipImageView_RT(int mip) {
        if (perMipImageViews.TryGetValue(mip, out var value)) {
            return value;
        }

        var device = VulkanContext.CurrentDevice.VkLogicalDevice;
        var imageViewCreateInfo = new ImageViewCreateInfo(StructureType.ImageViewCreateInfo) {
            ViewType = ImageViewType.Type2D,
            Format = Options.Format.ToVulkanImageFormat(),
            Image = ImageInfo.Image!.Value,
            SubresourceRange = new() {
                AspectMask = ImageAspectMask, BaseMipLevel = (uint)mip, LevelCount = 1, LayerCount = 1
            }
        };

        VulkanContext.Vulkan.CreateImageView(device, imageViewCreateInfo, null, out var imageView);
        VulkanUtils.SetDebugObjectName(
            ObjectType.ImageView,
            $"{Options.DebugName} image view mip: {mip}",
            imageView.Handle
        );

        perMipImageViews[mip] = imageView;
        return imageView;
    }

    void UpdateDescriptor() {
        if (Options.Format is ImageFormat.Depth24Stencil8 or ImageFormat.Depth32F or ImageFormat.Depth32FStencil8Uint) {
            descriptorImageInfo.ImageLayout = ImageLayout.DepthStencilReadOnlyOptimal;
        } else if (Options.Usage == ImageUsage.Storage) {
            descriptorImageInfo.ImageLayout = ImageLayout.General;
        } else {
            descriptorImageInfo.ImageLayout = ImageLayout.ShaderReadOnlyOptimal;
        }

        // TODO: this is overlapping previous statements
        if (Options.Usage == ImageUsage.Storage) {
            descriptorImageInfo.ImageLayout = ImageLayout.General;
        } else if (Options.Usage == ImageUsage.HostRead) {
            descriptorImageInfo.ImageLayout = ImageLayout.TransferDstOptimal;
        }

        descriptorImageInfo.ImageView = ImageInfo.ImageView!.Value;
        descriptorImageInfo.Sampler = ImageInfo.Sampler ?? default;
    }

    // TODO: CopyToHostBuffer

    public class VulkanImageInfo {
        public Image? Image { get; set; }
        public ImageView? ImageView { get; set; }
        public Sampler? Sampler { get; set; }
        public Allocation MemoryAllocation { get; set; }
    }
}
