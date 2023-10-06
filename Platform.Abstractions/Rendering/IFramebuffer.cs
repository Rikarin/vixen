namespace Rin.Platform.Abstractions.Rendering;

public interface IFramebuffer {
    FramebufferOptions Options { get; }
    int ColorAttachmentCount { get; }
    IImage2D? DepthImage { get; }
    
    IImage2D GetImage(int index);
}
