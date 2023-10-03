using Rin.Core.Abstractions;
using Silk.NET.Vulkan;
using System.Drawing;

namespace Rin.Platform.Rendering; 

public interface IFramebuffer {

}


public sealed class FramebufferOptions {
    public float Scale { get; set; }
    public Size? Size { get; set; }
    public Color ClearColor { get; set; } = Color.Black;
    public float DepthClearValue { get; set; }

    // TODO: use options pattern?
    public bool ClearColorOnLoad { get; set; } = true;
    public bool ClearDepthOnLoad { get; set; } = true;

    public FramebufferAttachmentOptions Attachments { get; set; }
    
    public bool Transfer { get; set; }
    
    
    // At the moment this will just create a new render pass
    // with an existing framebuffer
    public Framebuffer? ExistingFramebuffer { get; set; }
    public string DebugName { get; set; }
}

public sealed class FramebufferTextureOptions {
    public ImageFormat Format { get; set; }
    public bool Blend { get; set; } = true;
    public FramebufferBlendMode BlendMode { get; set; } = FramebufferBlendMode.SrcAlphaOneMinusSrcAlpha;
    public AttachmentLoadOp LoadOp { get; set; } = AttachmentLoadOp.Inherit;
}

public sealed class FramebufferAttachmentOptions {
    public List<FramebufferTextureOptions> Attachments { get; set; }
}






public enum FramebufferBlendMode {
    None,
    OneZero,
    SrcAlphaOneMinusSrcAlpha,
    Additive,
    ZeroSrcColor
}

public enum AttachmentLoadOp {
    Inherit, Clear, Load
}