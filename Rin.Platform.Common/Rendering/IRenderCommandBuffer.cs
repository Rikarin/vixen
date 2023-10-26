namespace Rin.Platform.Abstractions.Rendering; 

public interface IRenderCommandBuffer : IDisposable {
    void Begin();
    void End();
    void Submit();
}
