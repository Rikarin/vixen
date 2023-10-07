using Rin.Platform.Abstractions.Rendering;
using Rin.Platform.Silk;
using Rin.Rendering;
using Serilog;
using Silk.NET.Vulkan;

namespace Rin.Platform.Vulkan;

public sealed class VulkanRenderCommandBuffer : IRenderCommandBuffer {
    const int MaxUserQueries = 16;
    readonly string debugName;
    readonly bool ownedBySwapchain;

    CommandPool commandPool;
    int timestampNextAvailableQuery = 2;

    readonly ILogger log = Log.ForContext<IRenderCommandBuffer>();
    // readonly int pipelineQueryCount;
    readonly int timestampQueryCount;
    readonly List<QueryPool> timestampQueryPools = new();
    readonly List<QueryPool> pipelineStatisticsQueryPools = new();
    readonly List<List<int>> timestampQueryResults = new();
    readonly List<List<float>> gpuExecutionTimes = new();
    public CommandBuffer? ActiveCommandBuffer { get; private set; }

    public VulkanRenderCommandBuffer(int count, string debugName) {
        this.debugName = debugName;
        throw new NotImplementedException();
    }

    public unsafe VulkanRenderCommandBuffer(string debugName, bool ownedBySwapchain) {
        this.debugName = debugName;
        this.ownedBySwapchain = ownedBySwapchain;

        var vk = VulkanContext.Vulkan;
        var device = VulkanContext.CurrentDevice.VkLogicalDevice;
        var framesInFlight = Renderer.Options.FramesInFlight;

        timestampQueryCount = 2 + 2 * MaxUserQueries;
        var queryPoolCreateInfo = new QueryPoolCreateInfo(StructureType.QueryPoolCreateInfo) {
            QueryType = QueryType.Timestamp, QueryCount = (uint)timestampQueryCount
        };

        for (var i = 0; i < framesInFlight; i++) {
            vk.CreateQueryPool(device, queryPoolCreateInfo, null, out var queryPool).EnsureSuccess();
            timestampQueryPools.Add(queryPool);
            timestampQueryResults.Add(new());
            gpuExecutionTimes.Add(new());
        }

        // pipelineQueryCount = 7; // TODO: why it's 7??

        // TODO: statistics are not supported on MoltenVK
        // queryPoolCreateInfo.QueryType = QueryType.PipelineStatistics;
        // queryPoolCreateInfo.QueryCount = (uint)pipelineQueryCount;
        // queryPoolCreateInfo.PipelineStatistics =
        //     QueryPipelineStatisticFlags.InputAssemblyVerticesBit
        //     | QueryPipelineStatisticFlags.InputAssemblyPrimitivesBit
        //     | QueryPipelineStatisticFlags.VertexShaderInvocationsBit
        //     | QueryPipelineStatisticFlags.ClippingInvocationsBit
        //     | QueryPipelineStatisticFlags.ClippingPrimitivesBit
        //     | QueryPipelineStatisticFlags.FragmentShaderInvocationsBit
        //     | QueryPipelineStatisticFlags.ComputeShaderInvocationsBit;

        for (var i = 0; i < framesInFlight; i++) {
            vk.CreateQueryPool(device, queryPoolCreateInfo, null, out var queryPool).EnsureSuccess();
            pipelineStatisticsQueryPools.Add(queryPool);
        }
    }

    // TODO
    public unsafe void Begin() {
        timestampNextAvailableQuery = 2;

        Renderer.Submit(
            () => {
                log.Verbose("[Render Command Buffer] Begin");

                var vk = VulkanContext.Vulkan;
                var frameIndex = Renderer.CurrentFrameIndex_RT;
                var beginInfo = new CommandBufferBeginInfo(StructureType.CommandBufferBeginInfo) {
                    Flags = CommandBufferUsageFlags.OneTimeSubmitBit
                };

                if (ownedBySwapchain) {
                    var swapchain = SilkWindow.MainWindow.Swapchain as VulkanSwapChain; // TODO: fix this reference
                    ActiveCommandBuffer = swapchain.GetDrawCommandBuffer(frameIndex);
                } else {
                    // TODO
                    throw new NotImplementedException();
                }

                var cmdBuffer = ActiveCommandBuffer.Value;
                vk.BeginCommandBuffer(cmdBuffer, beginInfo).EnsureSuccess();

                // Timestamp query
                vk.CmdResetQueryPool(cmdBuffer, timestampQueryPools[frameIndex], 0, (uint)timestampQueryCount);
                vk.CmdWriteTimestamp(cmdBuffer, PipelineStageFlags.BottomOfPipeBit, timestampQueryPools[frameIndex], 0);

                // Pipeline stats query
                // vk.CmdResetQueryPool(cmdBuffer, pipelineStatisticsQueryPools[frameIndex], 0, (uint)pipelineQueryCount);
                // vk.CmdBeginQuery(cmdBuffer, pipelineStatisticsQueryPools[frameIndex], 0, 0);
            }
        );
    }

    public void End() {
        Renderer.Submit(
            () => {
                log.Verbose("[Render Command Buffer] End");

                var vk = VulkanContext.Vulkan;
                var frameIndex = Renderer.CurrentFrameIndex_RT;

                if (!ActiveCommandBuffer.HasValue) {
                    log.Warning("Ending non active buffer");
                    return;
                }

                var cmdBuffer = ActiveCommandBuffer.Value;
                // TODO: not present on molten vk
                vk.CmdWriteTimestamp(cmdBuffer, PipelineStageFlags.BottomOfPipeBit, timestampQueryPools[frameIndex], 1);
                // vk.CmdEndQuery(cmdBuffer, pipelineStatisticsQueryPools[frameIndex], 0);
                vk.EndCommandBuffer(cmdBuffer).EnsureSuccess();

                ActiveCommandBuffer = null;
            }
        );
    }

    public void Submit() {
        throw new NotImplementedException();
    }

    public unsafe void Dispose() {
        if (ownedBySwapchain) {
            return;
        }

        Renderer.SubmitDisposal(
            () => VulkanContext.Vulkan.DestroyCommandPool(
                VulkanContext.CurrentDevice.VkLogicalDevice,
                commandPool,
                null
            )
        );
    }
}
