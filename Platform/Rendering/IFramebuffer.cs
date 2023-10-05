namespace Rin.Platform.Rendering;

public interface IFramebuffer {
    public int ColorAttachmentCount { get; }
    IImage2D? DepthImage { get; }
    
    IImage2D GetImage(int index);
}
