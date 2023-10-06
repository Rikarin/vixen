namespace Rin.Platform.Abstractions.Rendering;

public interface IRenderer {
    public RenderingApi Api { get; }
    
    void Initialize();
    void Shutdown();
    void BeginFrame();
    void EndFrame();
    
    // TODO: other stuff

    void BeginRenderPass(IRenderCommandBuffer renderCommandBuffer, IRenderPass renderPass, bool explicitClear = false);
    void EndRenderPass(IRenderCommandBuffer renderCommandBuffer);
}