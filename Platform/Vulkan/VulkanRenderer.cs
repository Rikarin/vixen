using Rin.Platform.Abstractions.Rendering;
using Rin.Platform.Internal;
using Rin.Platform.Silk;
using Rin.Rendering;
using Serilog;
using Silk.NET.Vulkan;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Rin.Platform.Vulkan;

sealed class VulkanRenderer : IRenderer {
    readonly ILogger log = Log.ForContext<IRenderer>();
    readonly List<int> descriptorPoolAllocationCount = new();
    readonly List<DescriptorPool> descriptorPools = new();

    Sampler? samplerPoint;
    Sampler? samplerClamp;

    DescriptorPool materialDescriptorPool;

    int drawCallCount;


    IVertexBuffer quadVertexBuffer;
    IIndexBuffer quadIndexBuffer;


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

        // TODO: stuff


        var data = stackalloc[] {
            new QuadVertex(new(-1, -1, 0), Vector2.Zero), new QuadVertex(new(1, -1, 0), new(1, 0)),
            new QuadVertex(new(1, 1, 0), new(1, 1)), new QuadVertex(new(-1, 1, 0), new(0, 1))
        };

        quadVertexBuffer = ObjectFactory.CreateVertexBuffer(new ReadOnlySpan<byte>(data, 4 * sizeof(QuadVertex)));
        Debug.Assert(sizeof(QuadVertex) == 3 * 4 + 2 * 4);
        Debug.Assert(sizeof(QuadVertex) == Marshal.SizeOf<QuadVertex>());

        var indices = stackalloc[] { 0, 1, 2, 2, 3, 0 };
        quadIndexBuffer = ObjectFactory.CreateIndexBuffer(new(indices, 6 * sizeof(int)));

        // TODO: stuff
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
                    RenderPass = framebuffer.VkRenderPass
                };

                if (framebuffer.Options.IsSwapChainTarget) {
                    var swapchain = SilkWindow.MainWindow.Swapchain as VulkanSwapChain; // TODO
                    width = swapchain.Size.Width;
                    height = swapchain.Size.Height;

                    renderPassBeginInfo.Framebuffer = swapchain.CurrentFramebuffer;
                    
                    // TODO: adjust this to have +Y up
                    viewport.Y = swapchain.Size.Height;
                    viewport.Width = swapchain.Size.Width;
                    viewport.Height = -swapchain.Size.Height;

                    // viewport.Width = swapchain.Size.Width;
                    // viewport.Height = swapchain.Size.Height;
                } else {
                    renderPassBeginInfo.Framebuffer = framebuffer.VkFramebuffer.Value;

                    viewport.Width = framebuffer.Size.Width;
                    viewport.Height = framebuffer.Size.Height;
                }

                renderPassBeginInfo.RenderArea = new() { Extent = new((uint)width, (uint)height) };

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
                renderPass.Prepare();
                if (vkRenderPass.HasDescriptorSets) {
                    var descriptorSets = vkRenderPass.GetDescriptorSets(Renderer.CurrentFrameIndex_RT);
                    fixed (DescriptorSet* data = descriptorSets.ToArray()) {
                        vk.CmdBindDescriptorSets(
                            commandBuffer,
                            PipelineBindPoint.Graphics,
                            pipeline.VkLayout,
                            (uint)renderPass.FirstSetIndex,
                            (uint)descriptorSets.Count,
                            data,
                            0,
                            null
                        );
                    }
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

    public unsafe void RenderQuad(
        IRenderCommandBuffer commandBuffer,
        IPipeline pipeline,
        IMaterial material,
        Matrix4x4 transform
    ) {
        Renderer.Submit(
            () => {
                var vk = VulkanContext.Vulkan;
                var vkCommandBuffer = (commandBuffer as VulkanRenderCommandBuffer).ActiveCommandBuffer.Value;
                var vkLayout = (pipeline as VulkanPipeline).VkLayout;

                var offsets = new[] { 0ul };
                // ulong offset = 1ul;

                var vbMeshBuffer = (quadVertexBuffer as VulkanVertexBuffer).VkBuffer;
                // vk.CmdBindVertexBuffers(vkCommandBuffer, 0, &vbMeshBuffer, MemoryMarshal.CreateSpan(ref offset, 1));
                vk.CmdBindVertexBuffers(vkCommandBuffer, 0, &vbMeshBuffer, offsets);

                var ibMeshBuffer = (quadIndexBuffer as VulkanIndexBuffer).VkBuffer;
                vk.CmdBindIndexBuffer(vkCommandBuffer, ibMeshBuffer, 0, IndexType.Uint32);

                var transformSpan = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref transform, 1));
                vk.CmdPushConstants(
                    vkCommandBuffer,
                    vkLayout,
                    ShaderStageFlags.VertexBit,
                    0,
                    (uint)transformSpan.Length,
                    transformSpan
                );
                
                // var uniformStorageBuffer = (material as VulkanMaterial).UniformStorageBuffer;
                // using var uniformBufferMemoryHandle = uniformStorageBuffer.Pin();
                // vk.CmdPushConstants(
                //     vkCommandBuffer,
                //     vkLayout,
                //     ShaderStageFlags.FragmentBit,
                //     16u * sizeof(float),
                //     (uint)uniformStorageBuffer.Length,
                //     uniformBufferMemoryHandle.Pointer
                // );

                vk.CmdDrawIndexed(vkCommandBuffer, (uint)quadIndexBuffer.Count, 1, 0, 0, 0);
            }
        );
    }

    record struct QuadVertex(Vector3 Position, Vector2 TexCoord);
}
