namespace Rin.Platform.Abstractions.Rendering;

public interface IImage {
    void Invalidate();
    void Release();

    void CreatePerLayerImageViews();
}
