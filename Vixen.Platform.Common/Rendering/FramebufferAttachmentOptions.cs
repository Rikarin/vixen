using Vixen.Core.Common;

namespace Vixen.Platform.Common.Rendering;

public sealed class FramebufferAttachmentOptions {
    public List<FramebufferTextureOptions> Attachments { get; set; }

    public FramebufferAttachmentOptions(params ImageFormat[] imageFormat) {
        Attachments = new();
        foreach (var format in imageFormat) {
            Attachments.Add(new() { Format = format });
        }
    }
}
