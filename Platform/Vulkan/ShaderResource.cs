using Rin.Core.Abstractions.Shaders;
using Silk.NET.Vulkan;
using System.Runtime.Serialization;

namespace Rin.Platform.Vulkan;

// TODO: consider moving this outside of platform as there aren't many references on vulkan
static class ShaderResource {
    public sealed class ReflectionData {
        public Dictionary<int, ShaderDescriptorSet> ShaderDescriptorSets { get; set; } = new();
        public Dictionary<string, ShaderResourceDeclaration> Resources { get; set; } = new();
        public Dictionary<string, ShaderBuffer> ConstantBuffers { get; set; } = new();
        public List<PushConstantRange> PushConstantRanges { get; set; } = new();
    }

    public sealed class UniformBuffer {
        public DescriptorBufferInfo Descriptor { get; set; }
        public int Size { get; set; }
        public int BindingPoint { get; set; }
        public string Name { get; set; }

        public ShaderStageFlags ShaderStage { get; set; }
        // VkShaderStageFlagBits ShaderStage = VK_SHADER_STAGE_FLAG_BITS_MAX_ENUM; 
    }

    public sealed class StorageBuffer {
        public DescriptorBufferInfo Descriptor { get; set; }
        public int Size { get; set; }
        public int BindingPoint { get; set; }
        public string Name { get; set; }

        public ShaderStageFlags ShaderStage { get; set; }
        // VkShaderStageFlagBits ShaderStage = VK_SHADER_STAGE_FLAG_BITS_MAX_ENUM; 
    }

    public sealed class ImageSampler {
        public int BindingPoint { get; set; }
        public int DescriptorSet { get; set; }
        public int Dimension { get; set; }
        public int ArraySize { get; set; }
        public string Name { get; set; }
        public ShaderStageFlags ShaderStage { get; set; }
    }

    public sealed class PushConstantRange {
        public int Offset { get; set; }
        public int Size { get; set; }
        public ShaderStageFlags ShaderStage { get; set; }
    }

    public sealed class ShaderDescriptorSet {
        // was <uint, ...
        public Dictionary<int, UniformBuffer> UniformBuffers { get; set; } = new();
        public Dictionary<int, StorageBuffer> StorageBuffers { get; set; } = new();
        public Dictionary<int, ImageSampler> ImageSamplers { get; set; } = new();
        public Dictionary<int, ImageSampler> StorageImages { get; set; } = new();
        public Dictionary<int, ImageSampler> SeparateTextures { get; set; } = new();
        public Dictionary<int, ImageSampler> SeparateSamplers { get; set; } = new();


        [IgnoreDataMember]
        public Dictionary<string, WriteDescriptorSet> WriteDescriptorSets { get; set; } = new();
    }
}
