using Rin.Core.Abstractions;

namespace Rin.Platform.Abstractions.Rendering;

public interface IImage {
    ImageOptions Options { get; }
    
    void Invalidate();
    void Release();

    void CreatePerLayerImageViews();
}
