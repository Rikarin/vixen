using Rin.Core.Abstractions;
using Rin.Core.Abstractions.Shaders;
using Silk.NET.Vulkan;

namespace Rin.Platform.Vulkan; 

sealed class VulkanShader {
    public sealed class ReflectionData {
        public Dictionary<int, ShaderResource.ShaderDescriptorSet> ShaderDescriptorSets { get; } = new();
        public Dictionary<string, ShaderResourceDeclaration> Resources { get; } = new();
        public Dictionary<string, ShaderBuffer> ConstantBuffers { get; } = new();
        public List<ShaderResource.PushConstantRange> PushConstantRanges { get; } = new();
    }
}

static class ShaderResource {
    // [Serializable]
    public sealed class UniformBuffer {
        public DescriptorBufferInfo Descriptor { get; set; }
        public int Size { get; set; }
        public int BindingPoint { get; set; }
        public string Name { get; set; }
        public ShaderStageFlags ShaderStage { get; set; }
        // VkShaderStageFlagBits ShaderStage = VK_SHADER_STAGE_FLAG_BITS_MAX_ENUM; 
    }
    
    // [Serializable]
    public sealed class StorageBuffer {
        public DescriptorBufferInfo Descriptor { get; set; }
        public int Size { get; set; }
        public int BindingPoint { get; set; }
        public string Name { get; set; }
        public ShaderStageFlags ShaderStage { get; set; }
        // VkShaderStageFlagBits ShaderStage = VK_SHADER_STAGE_FLAG_BITS_MAX_ENUM; 
    }

    // [Serializable]
    public sealed class ImageSampler {
        public int BindingPoint { get; set; }
        public int DescriptorSet { get; set; }
        public int Dimension { get; set; }
        public int ArraySize { get; set; }
        public string Name { get; set; }
        public ShaderStageFlags ShaderStage { get; set; }
    }
    
    // [Serializable]
    public sealed class PushConstantRange {
        public int Offset { get; set; }
        public int Size { get; set; }
        public ShaderStageFlags ShaderStage { get; set; }
    }

    public sealed class ShaderDescriptorSet {
        // was <uint, ...
        public Dictionary<int, UniformBuffer> UniformBuffers { get; } = new();
        public Dictionary<int, StorageBuffer> StorageBuffers { get; } = new();
        public Dictionary<int, ImageSampler> ImageSamplers { get; } = new();
        public Dictionary<int, ImageSampler> StorageImages { get; } = new();
        public Dictionary<int, ImageSampler> SeparateTextures { get; } = new();
        public Dictionary<int, ImageSampler> SeparateSamplers { get; } = new();
    }
}