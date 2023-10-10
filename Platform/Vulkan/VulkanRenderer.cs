using Rin.Platform.Abstractions.Rendering;
using Rin.Platform.Silk;
using Rin.Rendering;
using Serilog;
using Silk.NET.Vulkan;

namespace Rin.Platform.Vulkan;

public sealed class VulkanRenderer : IRenderer {
    readonly ILogger log = Log.ForContext<IRenderer>();
    readonly List<int> descriptorPoolAllocationCount = new();
    readonly List<DescriptorPool> descriptorPools = new();

    Sampler? samplerPoint;
    Sampler? samplerClamp;

    DescriptorPool materialDescriptorPool;

    int drawCallCount;


    public RenderingApi Api => RenderingApi.Vulkan;

    public unsafe void Initialize() {
        var framesInFlight = Renderer.Options.FramesInFlight;


        // TODO stuff

        Renderer.Submit(
            () => {
                var vk = VulkanContext.Vulkan;

                // TODO: this is identical to DescriptorSetManager
                DescriptorPoolSize[] poolSizes = {
                    new(DescriptorType.Sampler, 1000), new(DescriptorType.CombinedImageSampler, 1000),
                    new(DescriptorType.SampledImage, 1000), new(DescriptorType.StorageImage, 1000),
                    new(DescriptorType.UniformTexelBuffer, 1000), new(DescriptorType.StorageTexelBuffer, 1000),
                    new(DescriptorType.UniformBuffer, 1000), new(DescriptorType.StorageBuffer, 1000),
                    new(DescriptorType.UniformBufferDynamic, 1000), new(DescriptorType.StorageBufferDynamic, 1000),
                    new(DescriptorType.InputAttachment, 1000)
                };

                fixed (DescriptorPoolSize* poolSizesPtr = poolSizes) {
                    var poolInfo = new DescriptorPoolCreateInfo(StructureType.DescriptorPoolCreateInfo) {
                        Flags = DescriptorPoolCreateFlags.FreeDescriptorSetBit,
                        MaxSets = 100000, // TODO: not sure about these constants
                        PoolSizeCount = (uint)poolSizes.Length,
                        PPoolSizes = poolSizesPtr
                    };

                    var device = VulkanContext.CurrentDevice.VkLogicalDevice;

                    for (var i = 0; i < framesInFlight; i++) {
                        vk.CreateDescriptorPool(device, poolInfo, null, out var pool).EnsureSuccess();
                        descriptorPools.Add(pool);
                        descriptorPoolAllocationCount.Add(0);
                    }

                    vk.CreateDescriptorPool(device, poolInfo, null, out var materialPool).EnsureSuccess();
                    materialDescriptorPool = materialPool;
                }
            }
        );
    }

    public void Shutdown() {
        var device = VulkanContext.CurrentDevice.VkLogicalDevice;
        VulkanContext.Vulkan.DeviceWaitIdle(device).EnsureSuccess();

        if (samplerPoint != null) {
            VulkanSampler.DestroySampler(samplerPoint.Value);
            samplerPoint = null;
        }

        if (samplerClamp != null) {
            VulkanSampler.DestroySampler(samplerClamp.Value);
            samplerClamp = null;
        }
    }

    public void BeginFrame() {
        Renderer.Submit(
            () => {
                log.Verbose("[Renderer] Begin Frame");
                var vk = VulkanContext.Vulkan;
                var device = VulkanContext.CurrentDevice.VkLogicalDevice;
                var swapchain = SilkWindow.MainWindow.Swapchain as VulkanSwapChain; // TODO
                var bufferIndex = swapchain.CurrentBufferIndex;

                vk.ResetDescriptorPool(device, descriptorPools[bufferIndex], 0).EnsureSuccess();
                descriptorPoolAllocationCount.Clear();
                drawCallCount = 0;
            }
        );
    }

    public void EndFrame() {
        Renderer.Submit(
            () =>
                log.Verbose("[Renderer] End Frame")
        );
    }

    public unsafe void BeginRenderPass(
        IRenderCommandBuffer renderCommandBuffer,
        IRenderPass renderPass,
        bool explicitClear = false
    ) {
        Renderer.Submit(
            () => {
                var vk = VulkanContext.Vulkan;
                log.Verbose("Begin Render Pass ({Name})", renderPass.Options.DebugName);

                var commandBuffer = (renderCommandBuffer as VulkanRenderCommandBuffer).ActiveCommandBuffer.Value;
                var framebuffer = renderPass.Options.Pipeline.Options.TargetFramebuffer as VulkanFramebuffer;

                var width = framebuffer.Size.Width;
                var height = framebuffer.Size.Height;

                var viewport = new Viewport { MinDepth = 0, MaxDepth = 1 };
                var renderPassBeginInfo = new RenderPassBeginInfo(StructureType.RenderPassBeginInfo) {
                    RenderPass = framebuffer.RenderPass
                };

                if (framebuffer.Options.IsSwapChainTarget) {
                    var swapchain = SilkWindow.MainWindow.Swapchain as VulkanSwapChain; // TODO
                    width = swapchain.Size.Width;
                    height = swapchain.Size.Height;
                    
                    renderPassBeginInfo.Framebuffer = swapchain.CurrentFramebuffer;

                    viewport.Y = swapchain.Size.Height;
                    viewport.Width = swapchain.Size.Width;
                    viewport.Height = -swapchain.Size.Height;
                } else {
                    renderPassBeginInfo.Framebuffer = framebuffer.vkFramebuffer.Value;
                    
                    viewport.Width = framebuffer.Size.Width;
                    viewport.Height = framebuffer.Size.Height;
                }
                
                renderPassBeginInfo.RenderArea = new() {
                    Extent = new((uint)width, (uint)height)
                };

                fixed (ClearValue* clearValues = framebuffer.ClearValues.ToArray()) {
                    renderPassBeginInfo.ClearValueCount = (uint)framebuffer.ClearValues.Count;
                    renderPassBeginInfo.PClearValues = clearValues;

                    vk.CmdBeginRenderPass(commandBuffer, renderPassBeginInfo, SubpassContents.Inline);
                }

                if (explicitClear) {
                    throw new NotImplementedException();
                }
                
                vk.CmdSetViewport(commandBuffer, 0, 1, viewport);

                var scissor = new Rect2D { Extent = new((uint)width, (uint)height) };
                vk.CmdSetScissor(commandBuffer, 0, 1, scissor);

                // Bind Vulkan Pipeline
                var pipeline = renderPass.Options.Pipeline as VulkanPipeline;
                vk.CmdBindPipeline(commandBuffer, PipelineBindPoint.Graphics, pipeline.VkPipeline);

                if (pipeline.IsDynamicLineWidth) {
                    vk.CmdSetLineWidth(commandBuffer, pipeline.Options.LineWidth);
                }

                var vkRenderPass = renderPass as VulkanRenderPass;
                // renderPass.Prepare();
                if (vkRenderPass.HasDescriptorSets) {
                    // TODO
                    // const auto& descriptorSets = vulkanRenderPass->GetDescriptorSets(frameIndex);
                    // vkCmdBindDescriptorSets(commandBuffer, VK_PIPELINE_BIND_POINT_GRAPHICS, vulkanPipeline->GetVulkanPipelineLayout(), vulkanRenderPass->GetFirstSetIndex(), (uint32_t)descriptorSets.size(), descriptorSets.data(), 0, nullptr);
                }
            }
        );
    }

    public void EndRenderPass(IRenderCommandBuffer renderCommandBuffer) {
        Renderer.Submit(
            () => {
                log.Verbose("[Renderer] End Render Pass");
                var frameIndex = Renderer.CurrentFrameIndex_RT;
                var commandBuffer = (renderCommandBuffer as VulkanRenderCommandBuffer).ActiveCommandBuffer;

                VulkanContext.Vulkan.CmdEndRenderPass(commandBuffer.Value);
                // TODO: debug stuff
            }
        );
    }
}
