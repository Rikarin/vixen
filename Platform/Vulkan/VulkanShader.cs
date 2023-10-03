using Rin.Platform.Rendering;
using Serilog;
using Silk.NET.Vulkan;
using System.Runtime.InteropServices;

namespace Rin.Platform.Vulkan;

public sealed class VulkanShader : IShader {
    ShaderCollection shaderData;

    readonly List<PipelineShaderStageCreateInfo> pipelineShaderStageCreateInfos = new();


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
}
