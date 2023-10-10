using System.Drawing;

namespace Rin.Platform.Abstractions.Rendering;

public interface IFramebuffer {
    FramebufferOptions Options { get; }
    Size Size { get; }
    int ColorAttachmentCount { get; }
    IImage2D? DepthImage { get; }
    
    IImage2D GetImage(int index);
}
