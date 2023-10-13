using System.Drawing;

namespace Rin.Platform.Abstractions.Rendering;

public sealed class FramebufferOptions {
    public float Scale { get; set; } = 1;
    public Size? Size { get; set; }
    public Color ClearColor { get; set; } = Color.Black;
    public float DepthClearValue { get; set; }

    // TODO: use options pattern?
    public bool ClearColorOnLoad { get; set; } = true;
    public bool ClearDepthOnLoad { get; set; } = true;
    public FramebufferAttachmentOptions Attachments { get; set; }

    public bool Blend { get; set; } = true;
    public FramebufferBlendMode BlendMode { get; set; }
    public bool IsSwapChainTarget { get; set; }
    public bool Transfer { get; set; }
    public IImage2D? ExistingImage { get; set; }
    public List<int> ExistingImageLayers { get; set; }
    public Dictionary<int, IImage2D>? ExistingImages { get; set; }


    // At the moment this will just create a new render pass
    // with an existing framebuffer
    public IFramebuffer? ExistingFramebuffer { get; set; }
    public string DebugName { get; set; }
}
