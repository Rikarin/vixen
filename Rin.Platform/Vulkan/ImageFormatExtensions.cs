using Rin.Core.Abstractions;
using Silk.NET.Vulkan;

namespace Rin.Platform.Vulkan;

static class ImageFormatExtensions {
    public static Format ToVulkanImageFormat(this ImageFormat format) =>
        format switch {
            ImageFormat.Red8Un => Format.R8Unorm,
            ImageFormat.Red8Ui => Format.R8Uint,
            ImageFormat.Red16Ui => Format.R16Uint,
            ImageFormat.Red32Ui => Format.R32Uint,
            ImageFormat.Red32F => Format.R32Sfloat,
            ImageFormat.Rg8 => Format.R8G8Unorm,
            ImageFormat.Rg16F => Format.R16G16Sfloat,
            ImageFormat.Rg32F => Format.R32G32Sfloat,
            ImageFormat.Rgba => Format.R8G8B8A8Unorm,
            ImageFormat.Rgba16F => Format.R16G16B16A16Sfloat,
            ImageFormat.Rgba32F => Format.R32G32B32A32Sfloat,
            ImageFormat.B10R11G11Uf => Format.B10G11R11UfloatPack32,
            ImageFormat.Depth32FStencil8Uint => Format.D32Sfloat,
            ImageFormat.Depth32F => Format.D32Sfloat,
            ImageFormat.Depth24Stencil8 => VulkanContext.CurrentDevice.PhysicalDevice.DepthFormat,
            _ => Format.Undefined
        };

    public static bool IsIntegerBased(this ImageFormat format) =>
        format switch {
            ImageFormat.Red16Ui => true,
            ImageFormat.Red32Ui => true,
            ImageFormat.Red8Ui => true,
            ImageFormat.Depth32FStencil8Uint => true,
            ImageFormat.Depth32F => false,
            ImageFormat.Red8Un => false,
            ImageFormat.Rgba32F => false,
            ImageFormat.B10R11G11Uf => false,
            ImageFormat.Rg16F => false,
            ImageFormat.Rg32F => false,
            ImageFormat.Red32F => false,
            ImageFormat.Rg8 => false,
            ImageFormat.Rgba => false,
            ImageFormat.Rgba16F => false,
            ImageFormat.Rgb => false,
            ImageFormat.Srgb => false,
            ImageFormat.Depth24Stencil8 => false,
            _ => false
        };
}
