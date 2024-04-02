namespace Vixen.Platform.Common.Rendering; 

public interface IRenderCommandBuffer : IDisposable {
    void Begin();
    void End();
    void Submit();
}
