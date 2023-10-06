using Rin.Core.Abstractions;
using Rin.Platform.Abstractions.Rendering;
using Serilog;

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

    // TODO: shutdown

    // TODO: make this internal
    public static void WaitAndRender(IRenderThread thread) {
        thread.WaitAndSet(RenderThreadState.Kick, RenderThreadState.Busy);
        // TODO: timers and stuff

        Log.Information("Rendering Idx {Index} Count {Count}", RenderQueueIndex, commandQueue[RenderQueueIndex].Count);
        commandQueue[RenderQueueIndex].Execute();
        thread.Set(RenderThreadState.Idle);
    }

    public static void SwapQueues() {
        Log.Information("Before Swapping queue: {Variable}", renderCommandQueueSubmissionIndex);
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
}

public class RendererOptions {
    public int FramesInFlight { get; set; }
}
