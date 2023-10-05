using Rin.Core.Abstractions;
using Rin.Platform.Rendering;
using Serilog;
using Silk.NET.Vulkan;
using System.Runtime.InteropServices;

namespace Rin.Platform.Vulkan;

sealed class VulkanShader : IShader, IDisposable {
    ShaderCollection shaderData;
    readonly List<PipelineShaderStageCreateInfo> pipelineShaderStageCreateInfos = new();

    readonly Dictionary<int, List<DescriptorPoolSize>> typeCounts = new();

    public ShaderResource.ReflectionData ReflectionData { get; internal set; }


    public unsafe void LoadAndCreateShaders(ShaderCollection data) {
        shaderData = data;
        pipelineShaderStageCreateInfos.Clear();

        // TODO: not deallocated yet
        var name = Marshal.StringToHGlobalAnsi("main");

        foreach (var entry in data) {
            using var ptr = entry.Value.Pin();
            var shaderModuleCreateInfo = new ShaderModuleCreateInfo(StructureType.ShaderModuleCreateInfo) {
                CodeSize = (uint)entry.Value.Length, PCode = (uint*)ptr.Pointer
            };

            var device = VulkanContext.CurrentDevice.VkLogicalDevice;
            VulkanContext.Vulkan.CreateShaderModule(device, shaderModuleCreateInfo, null, out var module);
            VulkanUtils.SetDebugObjectName(
                ObjectType.ShaderModule,
                $"Name:{data.Keys}",
                module.Handle
            );

            pipelineShaderStageCreateInfos.Add(
                new(StructureType.PipelineShaderStageCreateInfo) {
                    Stage = entry.Key, Module = module, PName = (byte*)name
                }
            );

            Log.Information("Debug: {Variable}", entry.Key);
        }

        // Marshal.FreeHGlobal(name);
    }

    // TODO: finish this
    public void Dispose() {
        Release();
    }

    public unsafe void CreateDescriptors() {
        var device = VulkanContext.CurrentDevice.VkLogicalDevice;

        foreach (var set in ReflectionData.ShaderDescriptorSets) {
            if (set.Value.UniformBuffers.Count > 0) {
                typeCounts
                    .GetOrCreateDefault(set.Key)
                    .Add(new(DescriptorType.UniformBuffer, (uint)set.Value.UniformBuffers.Count));
            }

            if (set.Value.StorageBuffers.Count > 0) {
                typeCounts
                    .GetOrCreateDefault(set.Key)
                    .Add(new(DescriptorType.StorageBuffer, (uint)set.Value.StorageBuffers.Count));
            }

            if (set.Value.ImageSamplers.Count > 0) {
                typeCounts
                    .GetOrCreateDefault(set.Key)
                    .Add(new(DescriptorType.CombinedImageSampler, (uint)set.Value.ImageSamplers.Count));
            }

            if (set.Value.SeparateTextures.Count > 0) {
                typeCounts
                    .GetOrCreateDefault(set.Key)
                    .Add(new(DescriptorType.SampledImage, (uint)set.Value.SeparateTextures.Count));
            }

            if (set.Value.SeparateSamplers.Count > 0) {
                typeCounts
                    .GetOrCreateDefault(set.Key)
                    .Add(new(DescriptorType.Sampler, (uint)set.Value.SeparateSamplers.Count));
            }

            if (set.Value.StorageImages.Count > 0) {
                typeCounts
                    .GetOrCreateDefault(set.Key)
                    .Add(new(DescriptorType.StorageImage, (uint)set.Value.StorageImages.Count));
            }

            // Descriptor Set Layout

            var layoutBindings = new List<DescriptorSetLayoutBinding>();
            foreach (var entry in set.Value.UniformBuffers) {
                layoutBindings.Add(
                    new() {
                        DescriptorType = DescriptorType.UniformBuffer,
                        DescriptorCount = 1,
                        StageFlags = entry.Value.ShaderStage,
                        Binding = (uint)entry.Key
                    }
                );

                set.Value.WriteDescriptorSets[entry.Value.Name] = new(StructureType.WriteDescriptorSet) {
                    DescriptorType = DescriptorType.UniformBuffer, DescriptorCount = 1, DstBinding = (uint)entry.Key
                };
            }

            foreach (var entry in set.Value.StorageBuffers) {
                layoutBindings.Add(
                    new() {
                        DescriptorType = DescriptorType.StorageBuffer,
                        DescriptorCount = 1,
                        StageFlags = entry.Value.ShaderStage,
                        Binding = (uint)entry.Key
                    }
                );

                set.Value.WriteDescriptorSets[entry.Value.Name] = new(StructureType.WriteDescriptorSet) {
                    DescriptorType = DescriptorType.StorageBuffer, DescriptorCount = 1, DstBinding = (uint)entry.Key
                };
            }

            foreach (var entry in set.Value.ImageSamplers) {
                layoutBindings.Add(
                    new() {
                        DescriptorType = DescriptorType.CombinedImageSampler,
                        DescriptorCount = (uint)entry.Value.ArraySize,
                        StageFlags = entry.Value.ShaderStage,
                        Binding = (uint)entry.Key
                    }
                );

                set.Value.WriteDescriptorSets[entry.Value.Name] = new(StructureType.WriteDescriptorSet) {
                    DescriptorType = DescriptorType.CombinedImageSampler,
                    DescriptorCount = (uint)entry.Value.ArraySize,
                    DstBinding = (uint)entry.Key
                };
            }

            foreach (var entry in set.Value.SeparateTextures) {
                layoutBindings.Add(
                    new() {
                        DescriptorType = DescriptorType.SampledImage,
                        DescriptorCount = (uint)entry.Value.ArraySize,
                        StageFlags = entry.Value.ShaderStage,
                        Binding = (uint)entry.Key
                    }
                );

                set.Value.WriteDescriptorSets[entry.Value.Name] = new(StructureType.WriteDescriptorSet) {
                    DescriptorType = DescriptorType.SampledImage,
                    DescriptorCount = (uint)entry.Value.ArraySize,
                    DstBinding = (uint)entry.Key
                };
            }

            foreach (var entry in set.Value.SeparateSamplers) {
                layoutBindings.Add(
                    new() {
                        DescriptorType = DescriptorType.Sampler,
                        DescriptorCount = (uint)entry.Value.ArraySize,
                        StageFlags = entry.Value.ShaderStage,
                        Binding = (uint)entry.Key
                    }
                );

                set.Value.WriteDescriptorSets[entry.Value.Name] = new(StructureType.WriteDescriptorSet) {
                    DescriptorType = DescriptorType.Sampler,
                    DescriptorCount = (uint)entry.Value.ArraySize,
                    DstBinding = (uint)entry.Key
                };
            }

            foreach (var entry in set.Value.StorageImages) {
                layoutBindings.Add(
                    new() {
                        DescriptorType = DescriptorType.StorageImage,
                        DescriptorCount = (uint)entry.Value.ArraySize,
                        StageFlags = entry.Value.ShaderStage,
                        Binding = (uint)entry.Key
                    }
                );

                set.Value.WriteDescriptorSets[entry.Value.Name] = new(StructureType.WriteDescriptorSet) {
                    DescriptorType = DescriptorType.StorageImage,
                    DescriptorCount = (uint)entry.Value.ArraySize,
                    DstBinding = (uint)entry.Key
                };
            }

            // TODO: not sure about this pinning
            fixed (DescriptorSetLayoutBinding* bindingsPtr = layoutBindings.ToArray()) {
                var descriptorLayout = new DescriptorSetLayoutCreateInfo(StructureType.DescriptorSetLayoutCreateInfo) {
                    BindingCount = (uint)layoutBindings.Count, PBindings = bindingsPtr
                };

                VulkanContext.Vulkan.CreateDescriptorSetLayout(device, descriptorLayout, null, out var setLayout);
                
                Log.Information(
                    "Creating descriptor set {Set} with {UniformBuffers} ubo's, {StorageBuffers} ssbo's, {ImageSamplers} samplers, {SeparateTextures} separate textures, {SeparateSamplers} separate samplers and {StorageImages} storage images",
                    set.Key,
                    set.Value.UniformBuffers.Count,
                    set.Value.StorageBuffers.Count,
                    set.Value.ImageSamplers.Count,
                    set.Value.SeparateTextures.Count,
                    set.Value.SeparateSamplers.Count,
                    set.Value.StorageImages.Count
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
