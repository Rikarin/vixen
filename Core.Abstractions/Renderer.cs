namespace Rin.Core.Abstractions;

public static class Renderer {
    static readonly RenderCommandQueue[] resourceFreeQueue = new RenderCommandQueue[3];

    // TODO: properly load this
    public static RendererOptions Options => new RendererOptions();

    static Renderer() {
        for (var i = 0; i < resourceFreeQueue.Length; i++) {
            resourceFreeQueue[i] = new();
        }
    }
    
    public static RenderCommandQueue GetRenderResourceReleaseQueue(int index) {
        return resourceFreeQueue[index];
    }


    public static void Submit(Action action) {
        // TODO
        // resourceFreeQueue
    }

    public static void SubmitDisposal(Action action) {
        // TODO
    }

    public static int CurrentFrameIndex => 0; // TODO
    public static int CurrentFrameIndex_RT => 0; // TODO
}


public class RendererOptions {
    public int FramesInFlight { get; set; }
}