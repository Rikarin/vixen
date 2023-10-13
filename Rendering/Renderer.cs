using Rin.Core.Abstractions;
using Rin.Platform.Abstractions.Diagnostics;
using Rin.Platform.Abstractions.Rendering;
using System.Numerics;

namespace Rin.Rendering;

public static class Renderer {
    static ICurrentBufferIndexAccessor currentBufferIndexAccessor;
    static int renderCommandQueueSubmissionIndex; // TODO: this was atomic but misused
    static readonly RenderCommandQueue[] commandQueue = new RenderCommandQueue[2];
    static readonly RenderCommandQueue[] resourceDisposeQueue = new RenderCommandQueue[3];

    static IRenderer renderer;

    public static RenderingApi CurrentApi => renderer.Api;

    // TODO: properly load this
    public static RendererOptions Options { get; private set; } = new();
    public static int CurrentFrameIndex { get; private set; }
    public static int CurrentFrameIndex_RT => currentBufferIndexAccessor.CurrentBufferIndex;
    public static int RenderQueueIndex => (renderCommandQueueSubmissionIndex + 1) % commandQueue.Length;
    public static RenderCommandQueue RenderCommandQueue => commandQueue[renderCommandQueueSubmissionIndex];


    // TODO: initialize these
    public static ITexture2D WhiteTexture => null;
    public static ITexture2D BlackTexture => null;
    public static ITextureCube BlackCubeTexture => null;

    public static void Initialize(ICurrentBufferIndexAccessor currentBufferIndexAccessor, IRenderer renderer) {
        Renderer.renderer = renderer;
        Renderer.currentBufferIndexAccessor = currentBufferIndexAccessor;

        commandQueue[0] = new();
        commandQueue[1] = new();

        // TODO: stuff
        for (var i = 0; i < resourceDisposeQueue.Length; i++) {
            resourceDisposeQueue[i] = new();
        }

        renderer.Initialize();
    }

    public static void Shutdown() {
        // TODO: shader dependencies
        renderer.Shutdown();
        // TODO

        for (var i = 0; i < Options.FramesInFlight; i++) {
            GetRenderDisposeQueue(i).Execute();
        }
    }

    // TODO: make this internal
    public static void WaitAndRender(IRenderThread thread) {
        using (var _ = RendererProfiling.StartWaitTime()) {
            thread.WaitAndSet(RenderThreadState.Kick, RenderThreadState.Busy);
        }

        RendererProfiling.SubmitCount.Record(commandQueue[RenderQueueIndex].Count);
        using (var _ = RendererProfiling.StartWorkTime()) {
            commandQueue[RenderQueueIndex].Execute();
            thread.Set(RenderThreadState.Idle);
        }
    }

    public static void DisposeQueue(int currentBufferIndex) {
        var queue = GetRenderDisposeQueue(currentBufferIndex);
        RendererProfiling.SubmitDisposalCount.Record(queue.Count);
        queue.Execute();
    }

    public static void SwapQueues() {
        renderCommandQueueSubmissionIndex = (renderCommandQueueSubmissionIndex + 1) % commandQueue.Length;
    }

    public static RenderCommandQueue GetRenderDisposeQueue(int index) => resourceDisposeQueue[index];

    public static void BeginFrame() => renderer.BeginFrame();
    public static void EndFrame() => renderer.EndFrame();

    public static void BeginRenderPass(
        IRenderCommandBuffer renderCommandBuffer,
        IRenderPass renderPass,
        bool explicitClear = false
    ) =>
        renderer.BeginRenderPass(renderCommandBuffer, renderPass, explicitClear);

    public static void EndRenderPass(IRenderCommandBuffer renderCommandBuffer) =>
        renderer.EndRenderPass(renderCommandBuffer);

    public static void IncreaseCurrentFrameIndex() {
        CurrentFrameIndex = (CurrentFrameIndex + 1) % Options.FramesInFlight;
    }

    public static void Submit(Action action) => RenderCommandQueue.Push(action);

    public static void SubmitDisposal(Action action) =>
        Submit(() => GetRenderDisposeQueue(CurrentFrameIndex_RT).Push(action));

    public static void RenderQuad(
        IRenderCommandBuffer commandBuffer,
        IPipeline pipeline,
        IMaterial material,
        Matrix4x4 transform
    ) =>
        renderer.RenderQuad(commandBuffer, pipeline, material, transform);
}

public class RendererOptions {
    public int FramesInFlight { get; set; }
}
