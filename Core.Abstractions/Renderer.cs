using System.Runtime.CompilerServices;

namespace Rin.Core.Abstractions;

public static class Renderer {
    static ICurrentBufferIndexAccessor currentBufferIndexAccessor;
    static int renderCommandQueueSubmissionIndex; // TODO: this was atomic but misused
    static readonly RenderCommandQueue[] commandQueue = new RenderCommandQueue[2];
    static readonly RenderCommandQueue[] resourceDisposeQueue = new RenderCommandQueue[3];

    // TODO: properly load this
    public static RendererOptions Options { get; private set; } = new();

    public static int CurrentFrameIndex { get; private set; }
    public static int CurrentFrameIndex_RT => currentBufferIndexAccessor.CurrentBufferIndex;

    public static int RenderQueueIndex => (renderCommandQueueSubmissionIndex + 1) % commandQueue.Length;
    public static RenderCommandQueue RenderCommandQueue => commandQueue[renderCommandQueueSubmissionIndex];

    public static void Initialize(ICurrentBufferIndexAccessor currentBufferIndexAccessor) {
        Renderer.currentBufferIndexAccessor = currentBufferIndexAccessor;
        commandQueue[0] = new();
        commandQueue[1] = new();

        // TODO: stuff
        for (var i = 0; i < resourceDisposeQueue.Length; i++) {
            resourceDisposeQueue[i] = new();
        }
    }

    // TODO: make this internal
    public static void WaitAndRender(IRenderThread thread) {
        thread.WaitAndSet(RenderThreadState.Kick, RenderThreadState.Busy);
        // TODO: timers and stuff

        commandQueue[RenderQueueIndex].Execute();
        thread.Set(RenderThreadState.Idle);
    }

    public static void SwapQueues() {
        renderCommandQueueSubmissionIndex = (renderCommandQueueSubmissionIndex + 1) % commandQueue.Length;
    }

    public static RenderCommandQueue GetRenderDisposeQueue(int index) => resourceDisposeQueue[index];

    // public void BeginFrame() => Renderera

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
