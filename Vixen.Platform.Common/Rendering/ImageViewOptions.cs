namespace Vixen.Platform.Common.Rendering;

public sealed class ImageViewOptions {
    public IImage2D Image { get; set; }
    public int Mip { get; set; }
    public string DebugName { get; set; }
}
