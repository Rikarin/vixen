using System.Numerics;

namespace Rin.Platform.Abstractions.Rendering;

public interface IRenderer {
    RenderingApi Api { get; }
    
    void Initialize();
    void Shutdown();
    void BeginFrame();
    void EndFrame();
    
    // TODO: other stuff
    void RenderQuad(IRenderCommandBuffer commandBuffer, IPipeline pipeline, IMaterial material, Matrix4x4 transform);

    void BeginRenderPass(IRenderCommandBuffer renderCommandBuffer, IRenderPass renderPass, bool explicitClear = false);
    void EndRenderPass(IRenderCommandBuffer renderCommandBuffer);
}