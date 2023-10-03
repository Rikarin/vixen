using Rin.Core.Abstractions;
using Rin.Platform.Internal;
using Rin.Platform.Rendering;
using Silk.NET.Vulkan;
using System.Drawing;

namespace Rin.Platform.Vulkan;

public sealed class VulkanFramebuffer : IFramebuffer, IDisposable {
    FramebufferOptions options;
    RenderPass renderPass;
    Framebuffer framebuffer;
    readonly Size size;

    readonly List<IImage2D> attachmentImages = new();

    public IImage2D DepthAttachmentImage { get; private set; }

    // TODO: others

    public VulkanFramebuffer(FramebufferOptions options) {
        this.options = options;

        if (options.Size == null) {
            // TODO
        } else {
            size = new(
                (int)(options.Size.Value.Width * options.Scale),
                (int)(options.Size.Value.Height * options.Scale)
            );
        }

        var attachmentIndex = 0;
        if (options.ExistingFramebuffer == null) {
            foreach (var attachment in options.Attachments.Attachments) {
                if (options.ExistingImage != null) {
                    if (attachment.Format.IsDepthFormat()) {
                        DepthAttachmentImage = options.ExistingImage;
                    } else {
                        attachmentImages.Add(options.ExistingImage);
                    }
                } else if (
                    options.ExistingImages != null
                    && options.ExistingImages.TryGetValue(attachmentIndex, out var value)
                ) {
                    if (attachment.Format.IsDepthFormat()) {
                        DepthAttachmentImage = value;
                    } else {
                        attachmentImages.Add(null); // TODO: not sure about this
                    }
                } else if (attachment.Format.IsDepthFormat()) {
                    var imageOptions = new ImageOptions {
                        Format = attachment.Format,
                        Usage = ImageUsage.Attachment,
                        Transfer = options.Transfer,
                        Size = size, // TODO: check as this is implemented differently
                        DebugName = $"{options.DebugName ?? "Unknown FrameBuffer"}-DepthAttachment{attachmentIndex}"
                    };
                    DepthAttachmentImage = ObjectFactory.CreateImage2D(imageOptions);
                } else {
                    var imageOptions = new ImageOptions {
                        Format = attachment.Format,
                        Usage = ImageUsage.Attachment,
                        Transfer = options.Transfer,
                        Size = size, // TODO: check as this is implemented differently
                        DebugName = $"{options.DebugName ?? "Unknown FrameBuffer"}-ColorAttachment{attachmentIndex}"
                    };
                    attachmentImages.Add(ObjectFactory.CreateImage2D(imageOptions));
                }

                attachmentIndex++;
            }
        }

        Resize(size, true);
    }

    public void Dispose() {
        Release();
    }


    void Resize(Size newSize, bool forceRecreate) {
        // TODO
    }

    void Release() {
        // TODO
    }

    public void Invalidate() => Renderer.Submit(Invalidate_RT);

    void Invalidate_RT() {
        // TODO
    }
}
