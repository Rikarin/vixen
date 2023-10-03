using Rin.Core.Abstractions;
using Rin.Platform.Rendering;
using Silk.NET.Vulkan;
using PrimitiveTopology = Rin.Platform.Rendering.PrimitiveTopology;

namespace Rin.Platform.Vulkan;

public sealed class VulkanPipeline : IPipeline, IDisposable {
    readonly PipelineOptions options;
    Pipeline pipeline;
    PipelineLayout layout;
    PipelineCache cache;

    public bool IsDynamicLineWidth =>
        options.Topology is PrimitiveTopology.Lines or PrimitiveTopology.LineStrip || options.WireFrame;

    public VulkanPipeline(PipelineOptions options) {
        this.options = options;
        Invalidate();

        // TODO: Register Shader Dependency
    }

    public unsafe void Dispose() {
        Renderer.SubmitDisposal(
            () => {
                var device = VulkanContext.CurrentDevice.VkLogicalDevice;
                var vk = VulkanContext.Vulkan;

                vk.DestroyPipeline(device, pipeline, null);
                vk.DestroyPipelineCache(device, cache, null);
                vk.DestroyPipelineLayout(device, layout, null);
            }
        );
    }


    void Invalidate() {
        Renderer.Submit(
            () => {
                var device = VulkanContext.CurrentDevice.VkLogicalDevice;
                var vk = VulkanContext.Vulkan;

                var shader = options.Shader as VulkanShader;
                var framebuffer = options.TargetFramebuffer as VulkanFramebuffer;
                
                // var descriptorSetLayouts = shader.desc
                // TODO: finish this after VkShader is done
            });
    }
}
