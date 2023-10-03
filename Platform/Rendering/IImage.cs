namespace Rin.Platform.Rendering; 

public interface IImage {
    void Invalidate();
    void Release();

    void CreatePerLayerImageViews();
}