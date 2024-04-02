using Vixen.Core.Common;

namespace Vixen.Platform.Common.Rendering;

public interface IImage {
    ImageOptions Options { get; }
    
    void Invalidate();
    void Release();

    void CreatePerLayerImageViews();
}
