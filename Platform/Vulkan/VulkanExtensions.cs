using Rin.Platform.Abstractions.Rendering;
using Silk.NET.Vulkan;
using PrimitiveTopology = Rin.Platform.Abstractions.Rendering.PrimitiveTopology;
using VkPrimitiveTopology = Silk.NET.Vulkan.PrimitiveTopology;

namespace Rin.Platform.Vulkan;

static class VulkanExtensions {
    public static RenderPassInputType ToRenderPassInputType(this DescriptorType type) =>
        type switch {
            DescriptorType.CombinedImageSampler => RenderPassInputType.ImageSampler2D,
            DescriptorType.SampledImage => RenderPassInputType.ImageSampler2D,
            DescriptorType.StorageImage => RenderPassInputType.StorageImage2D,
            DescriptorType.UniformBuffer => RenderPassInputType.UniformBuffer,
            DescriptorType.StorageBuffer => RenderPassInputType.StorageBuffer,
            _ => RenderPassInputType.None
        };

    public static RenderPassResourceType GetDefaultResourceType(this DescriptorType type) =>
        type switch {
            DescriptorType.CombinedImageSampler => RenderPassResourceType.Texture2D,
            DescriptorType.SampledImage => RenderPassResourceType.Texture2D,
            DescriptorType.StorageImage => RenderPassResourceType.Image2D,
            DescriptorType.UniformBuffer => RenderPassResourceType.UniformBuffer,
            DescriptorType.StorageBuffer => RenderPassResourceType.StorageBuffer,
            _ => RenderPassResourceType.None
        };

    public static VkPrimitiveTopology ToVulkan(this PrimitiveTopology topology) =>
        topology switch {
            PrimitiveTopology.Points => VkPrimitiveTopology.PointList,
            PrimitiveTopology.Lines => VkPrimitiveTopology.LineList,
            PrimitiveTopology.Triangles => VkPrimitiveTopology.TriangleList,
            PrimitiveTopology.LineStrip => VkPrimitiveTopology.LineStrip,
            PrimitiveTopology.TriangleStrip => VkPrimitiveTopology.TriangleStrip,
            PrimitiveTopology.TriangleFan => VkPrimitiveTopology.TriangleFan,
            _ => throw new ArgumentOutOfRangeException()
        };

    public static CompareOp ToVulkan(this DepthCompareOperator compareOperator) =>
        compareOperator switch {
            DepthCompareOperator.Never => CompareOp.Never,
            DepthCompareOperator.NotEqual => CompareOp.NotEqual,
            DepthCompareOperator.Less => CompareOp.Less,
            DepthCompareOperator.LessOrEqual => CompareOp.LessOrEqual,
            DepthCompareOperator.Greater => CompareOp.Greater,
            DepthCompareOperator.GreaterOrEqual => CompareOp.GreaterOrEqual,
            DepthCompareOperator.Equal => CompareOp.Equal,
            DepthCompareOperator.Always => CompareOp.Always,
            _ => throw new ArgumentOutOfRangeException()
        };

    public static Format ToVulkan(this ShaderDataType type) =>
        type switch {
            ShaderDataType.Float => Format.R32Sfloat,
            ShaderDataType.Float2 => Format.R32G32Sfloat,
            ShaderDataType.Float3 => Format.R32G32B32Sfloat,
            ShaderDataType.Float4 => Format.R32G32B32A32Sfloat,
            ShaderDataType.Int => Format.R32Sint,
            ShaderDataType.Int2 => Format.R32G32Sint,
            ShaderDataType.Int3 => Format.R32G32B32Sint,
            ShaderDataType.Int4 => Format.R32G32B32A32Sint,
            _ => Format.Undefined
        };
}
