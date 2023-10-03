using Rin.Core.Abstractions;

namespace Rin.Platform.Rendering;

public sealed class FramebufferTextureOptions {
    public ImageFormat Format { get; set; }
    public bool Blend { get; set; } = true;
    public FramebufferBlendMode BlendMode { get; set; } = FramebufferBlendMode.SrcAlphaOneMinusSrcAlpha;
    public AttachmentLoadOp LoadOp { get; set; } = AttachmentLoadOp.Inherit;
}
