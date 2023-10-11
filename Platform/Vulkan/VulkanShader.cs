using Rin.Core.Abstractions;
using Rin.Platform.Abstractions.Rendering;
using Rin.Rendering;
using Serilog;
using Silk.NET.Vulkan;
using System.Runtime.InteropServices;

namespace Rin.Platform.Vulkan;

public sealed class VulkanShader : IShader, IDisposable {
    readonly ILogger log = Log.ForContext<IShader>();
    readonly List<PipelineShaderStageCreateInfo> pipelineShaderStageCreateInfos = new();

    readonly Dictionary<int, List<DescriptorPoolSize>> typeCounts = new();
    readonly Dictionary<int, DescriptorSetLayout> descriptorSetLayouts = new();

    public string Name { get; }
    public ShaderResource.ReflectionData ReflectionData { get; internal set; }
    public IReadOnlyDictionary<int, DescriptorSetLayout> DescriptorSetLayouts => descriptorSetLayouts.AsReadOnly();

    public IReadOnlyDictionary<int, ShaderResource.ShaderDescriptorSet> ShaderDescriptorSets =>
        ReflectionData.ShaderDescriptorSets.AsReadOnly();

    public IReadOnlyList<ShaderResource.PushConstantRange> PushConstantRanges =>
        ReflectionData.PushConstantRanges.AsReadOnly();

    public IReadOnlyList<PipelineShaderStageCreateInfo> PipelineShaderStageCreateInfos =>
        pipelineShaderStageCreateInfos;


    public VulkanShader(string name) {
        Name = name;
    }

    public unsafe void LoadAndCreateShaders(ShaderCollection data) {
        pipelineShaderStageCreateInfos.Clear();

        // TODO: not deallocated yet
        // TODO: extend ShaderCollection and load these from compiler
        var vertName = Marshal.StringToHGlobalAnsi("vert");
        var fragName = Marshal.StringToHGlobalAnsi("frag");

        foreach (var (shaderStage, shaderData) in data) {
            using var ptr = shaderData.Pin();
            var shaderModuleCreateInfo = new ShaderModuleCreateInfo(StructureType.ShaderModuleCreateInfo) {
                CodeSize = (uint)shaderData.Length, PCode = (uint*)ptr.Pointer
            };

            var device = VulkanContext.CurrentDevice.VkLogicalDevice;
            VulkanContext.Vulkan.CreateShaderModule(device, shaderModuleCreateInfo, null, out var module)
                .EnsureSuccess();
            VulkanUtils.SetDebugObjectName(
                ObjectType.ShaderModule,
                $"{Name}:{shaderStage}",
                module.Handle
            );

            var entryPoint = shaderStage == ShaderStage.Vertex ? vertName : fragName;
            pipelineShaderStageCreateInfos.Add(
                new(StructureType.PipelineShaderStageCreateInfo) {
                    Stage = shaderStage.ToVulkan(), Module = module, PName = (byte*)entryPoint
                }
            );
        }
    }

    // TODO: finish this
    public void Dispose() {
        Release();
    }

    public unsafe void CreateDescriptors() {
        var device = VulkanContext.CurrentDevice.VkLogicalDevice;

        foreach (var (set, data) in ReflectionData.ShaderDescriptorSets) {
            if (data.UniformBuffers.Count > 0) {
                typeCounts
                    .GetOrCreateDefault(set)
                    .Add(new(DescriptorType.UniformBuffer, (uint)data.UniformBuffers.Count));
            }

            if (data.StorageBuffers.Count > 0) {
                typeCounts
                    .GetOrCreateDefault(set)
                    .Add(new(DescriptorType.StorageBuffer, (uint)data.StorageBuffers.Count));
            }

            if (data.ImageSamplers.Count > 0) {
                typeCounts
                    .GetOrCreateDefault(set)
                    .Add(new(DescriptorType.CombinedImageSampler, (uint)data.ImageSamplers.Count));
            }

            if (data.SeparateTextures.Count > 0) {
                typeCounts
                    .GetOrCreateDefault(set)
                    .Add(new(DescriptorType.SampledImage, (uint)data.SeparateTextures.Count));
            }

            if (data.SeparateSamplers.Count > 0) {
                typeCounts
                    .GetOrCreateDefault(set)
                    .Add(new(DescriptorType.Sampler, (uint)data.SeparateSamplers.Count));
            }

            if (data.StorageImages.Count > 0) {
                typeCounts
                    .GetOrCreateDefault(set)
                    .Add(new(DescriptorType.StorageImage, (uint)data.StorageImages.Count));
            }

            // Descriptor Set Layout
            var layoutBindings = new List<DescriptorSetLayoutBinding>();
            foreach (var (binding, buffer) in data.UniformBuffers) {
                layoutBindings.Add(
                    new() {
                        DescriptorType = DescriptorType.UniformBuffer,
                        DescriptorCount = 1,
                        StageFlags = buffer.ShaderStage,
                        Binding = (uint)binding
                    }
                );

                data.WriteDescriptorSets[buffer.Name] = new(StructureType.WriteDescriptorSet) {
                    DescriptorType = DescriptorType.UniformBuffer, DescriptorCount = 1, DstBinding = (uint)binding
                };
            }

            foreach (var (binding, buffer) in data.StorageBuffers) {
                layoutBindings.Add(
                    new() {
                        DescriptorType = DescriptorType.StorageBuffer,
                        DescriptorCount = 1,
                        StageFlags = buffer.ShaderStage,
                        Binding = (uint)binding
                    }
                );

                data.WriteDescriptorSets[buffer.Name] = new(StructureType.WriteDescriptorSet) {
                    DescriptorType = DescriptorType.StorageBuffer, DescriptorCount = 1, DstBinding = (uint)binding
                };
            }

            foreach (var (binding, sampler) in data.ImageSamplers) {
                layoutBindings.Add(
                    new() {
                        DescriptorType = DescriptorType.CombinedImageSampler,
                        DescriptorCount = (uint)sampler.ArraySize,
                        StageFlags = sampler.ShaderStage,
                        Binding = (uint)binding
                    }
                );

                data.WriteDescriptorSets[sampler.Name] = new(StructureType.WriteDescriptorSet) {
                    DescriptorType = DescriptorType.CombinedImageSampler,
                    DescriptorCount = (uint)sampler.ArraySize,
                    DstBinding = (uint)binding
                };
            }

            foreach (var (binding, sampler) in data.SeparateTextures) {
                layoutBindings.Add(
                    new() {
                        DescriptorType = DescriptorType.SampledImage,
                        DescriptorCount = (uint)sampler.ArraySize,
                        StageFlags = sampler.ShaderStage,
                        Binding = (uint)binding
                    }
                );

                data.WriteDescriptorSets[sampler.Name] = new(StructureType.WriteDescriptorSet) {
                    DescriptorType = DescriptorType.SampledImage,
                    DescriptorCount = (uint)sampler.ArraySize,
                    DstBinding = (uint)binding
                };
            }

            foreach (var (binding, sampler) in data.SeparateSamplers) {
                layoutBindings.Add(
                    new() {
                        DescriptorType = DescriptorType.Sampler,
                        DescriptorCount = (uint)sampler.ArraySize,
                        StageFlags = sampler.ShaderStage,
                        Binding = (uint)binding
                    }
                );

                data.WriteDescriptorSets[sampler.Name] = new(StructureType.WriteDescriptorSet) {
                    DescriptorType = DescriptorType.Sampler,
                    DescriptorCount = (uint)sampler.ArraySize,
                    DstBinding = (uint)binding
                };
            }

            foreach (var (binding, sampler) in data.StorageImages) {
                layoutBindings.Add(
                    new() {
                        DescriptorType = DescriptorType.StorageImage,
                        DescriptorCount = (uint)sampler.ArraySize,
                        StageFlags = sampler.ShaderStage,
                        Binding = (uint)binding
                    }
                );

                data.WriteDescriptorSets[sampler.Name] = new(StructureType.WriteDescriptorSet) {
                    DescriptorType = DescriptorType.StorageImage,
                    DescriptorCount = (uint)sampler.ArraySize,
                    DstBinding = (uint)binding
                };
            }

            // TODO: not sure about this pinning
            fixed (DescriptorSetLayoutBinding* bindingsPtr = layoutBindings.ToArray()) {
                var descriptorLayout = new DescriptorSetLayoutCreateInfo(StructureType.DescriptorSetLayoutCreateInfo) {
                    BindingCount = (uint)layoutBindings.Count, PBindings = bindingsPtr
                };

                VulkanContext.Vulkan.CreateDescriptorSetLayout(device, descriptorLayout, null, out var setLayout)
                    .EnsureSuccess();
                descriptorSetLayouts[set] = setLayout;

                log.Debug(
                    "Creating descriptor set {Set} with {UniformBuffers} ubo's, {StorageBuffers} ssbo's, {ImageSamplers} samplers, {SeparateTextures} separate textures, {SeparateSamplers} separate samplers and {StorageImages} storage images",
                    set,
                    data.UniformBuffers.Count,
                    data.StorageBuffers.Count,
                    data.ImageSamplers.Count,
                    data.SeparateTextures.Count,
                    data.SeparateSamplers.Count,
                    data.StorageImages.Count
                );
            }
        }
    }

    // TODO: create descriptors

    unsafe void Release() {
        Renderer.SubmitDisposal(
            () => {
                var device = VulkanContext.CurrentDevice.VkLogicalDevice;
                foreach (var ci in pipelineShaderStageCreateInfos) {
                    VulkanContext.Vulkan.DestroyShaderModule(device, ci.Module, null);
                }

                pipelineShaderStageCreateInfos.Clear();
                // descriptorser TODO
                typeCounts.Clear();
            }
        );
    }
}

public static class ShaderStagesExtensions {
    public static ShaderStageFlags ToVulkan(this ShaderStage shaderStage) =>
        shaderStage switch {
            ShaderStage.Vertex => ShaderStageFlags.VertexBit,
            ShaderStage.Fragment => ShaderStageFlags.FragmentBit
        };
}
