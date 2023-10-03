namespace Rin.Platform.Rendering; 

public interface IImage {
    void Invalidate();
    void Release();

    void CreatePerLayerImageViews();
}


public interface IImage2D : IImage {
    
}

public interface IImageView {
    
}

public sealed class ImageViewOptions {
    public IImage2D Image { get; set; }
    public int Mip { get; set; }
    public string DebugName { get; set; }
}