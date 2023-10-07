using Rin.Core.Abstractions;
using Rin.Platform.Abstractions.Rendering;
using Rin.Platform.Internal;
using Rin.Platform.Silk;
using Rin.Rendering;
using Silk.NET.Vulkan;
using System.Drawing;
using AttachmentLoadOp = Silk.NET.Vulkan.AttachmentLoadOp;

namespace Rin.Platform.Vulkan;

public sealed class VulkanFramebuffer : IFramebuffer, IDisposable {
    Framebuffer? framebuffer;
    Size size;

    // Not sure if use list or dictionary
    readonly List<IImage2D> attachmentImages = new();

    public List<ClearValue> ClearValues { get; } = new();
    public FramebufferOptions Options { get; }
    public IImage2D? DepthImage { get; private set; }
    public RenderPass RenderPass { get; private set; }

    public int ColorAttachmentCount => Options.IsSwapChainTarget ? 1 : attachmentImages.Count;

    public VulkanFramebuffer(FramebufferOptions options) {
        Options = options;

        if (options.Size == null) {
            size = SilkWindow.MainWindow.Size;
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
                        DepthImage = options.ExistingImage;
                    } else {
                        attachmentImages.Add(options.ExistingImage);
                    }
                } else if (
                    options.ExistingImages != null
                    && options.ExistingImages.TryGetValue(attachmentIndex, out var value)
                ) {
                    if (attachment.Format.IsDepthFormat()) {
                        DepthImage = value;
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
                    DepthImage = ObjectFactory.CreateImage2D(imageOptions);
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

    public void Dispose() => Release();
    public IImage2D GetImage(int index) => attachmentImages[index];

    void Resize(Size newSize, bool forceRecreate) {
        if (!forceRecreate && newSize == size) {
            return;
        }

        Renderer.Submit(
            () => {
                size = newSize;
                if (!Options.IsSwapChainTarget) {
                    Invalidate_RT();
                } else {
                    var vkSwapchain = SilkWindow.MainWindow.Swapchain as VulkanSwapChain; // TODO
                    RenderPass = vkSwapchain.RenderPass.Value;

                    ClearValues.Clear();
                    ClearValues.Add(new(new(0, 0, 0, 1)));
                }
            }
        );
    }

    unsafe void Release() {
        if (framebuffer == null) {
            return;
        }

        Renderer.SubmitDisposal(
            () => VulkanContext.Vulkan.DestroyFramebuffer(
                VulkanContext.CurrentDevice.VkLogicalDevice,
                framebuffer.Value,
                null
            )
        );

        if (Options.ExistingFramebuffer == null) {
            // TODO: check and verify this
            // var attachmentIndex = 0;
            // foreach (var image in attachmentImages) {
            //     if (options.ExistingImages.ContainsKey(attachmentIndex)) {
            //         continue;
            //     }
            // }
        }

        if (DepthImage != null) {
            // TODO
        }
    }

    unsafe void Invalidate_RT() {
        // TODO: this is not called right now
        throw new NotImplementedException();

        var device = VulkanContext.CurrentDevice.VkLogicalDevice;
        var vk = VulkanContext.Vulkan;

        Release();

        var attachmentDescriptions = new List<AttachmentDescription>();
        var colorAttachmentReferences = new List<AttachmentReference>();
        // var depthAttachmentReference = new De

        // TODO: this was resized
        ClearValues.Clear();

        if (Options.ExistingFramebuffer != null) {
            attachmentImages.Clear();
        }

        for (var attachmentIndex = 0; attachmentIndex < Options.Attachments.Attachments.Count; attachmentIndex++) {
            var attachment = Options.Attachments.Attachments[attachmentIndex];

            if (attachment.Format.IsDepthFormat()) {
                if (Options.ExistingImage != null) {
                    DepthImage = Options.ExistingImage;
                } else if (Options.ExistingFramebuffer != null) {
                    var fb = Options.ExistingFramebuffer as VulkanFramebuffer;
                    DepthImage = fb.DepthImage;
                } else if (Options.ExistingImages.TryGetValue(attachmentIndex, out var existingImage)) {
                    DepthImage = existingImage;
                } else {
                    DepthImage = new VulkanImage2D(new() { Size = size }); // TODO: not sure about this size
                    DepthImage.Invalidate();
                }

                var loadOp = GetAttachmentLoadOp(attachment);
                attachmentDescriptions.Add(
                    new() {
                        Format = attachment.Format.ToVulkanImageFormat(),
                        Samples = SampleCountFlags.Count1Bit,
                        LoadOp = loadOp,
                        StoreOp = AttachmentStoreOp.Store,
                        StencilLoadOp = AttachmentLoadOp.DontCare,
                        StencilStoreOp = AttachmentStoreOp.DontCare,
                        InitialLayout =
                            loadOp == AttachmentLoadOp.Clear
                                ? ImageLayout.Undefined
                                : ImageLayout.DepthStencilReadOnlyOptimal,
                        FinalLayout = ImageLayout.ReadOnlyOptimal // TODO: check this as it was implemented differently
                    }
                );

                // TODO: depth stencil reference
                ClearValues.Add(new() { DepthStencil = new(Options.DepthClearValue, 0) });
            } else {
                throw new NotImplementedException();
            }
        }


        // TODO: stuff


        // Create Render Pass
        var renderPassCreateInfo = new RenderPassCreateInfo(StructureType.RenderPassCreateInfo) {
            // TODO
            // AttachmentCount = 0
        };

        vk.CreateRenderPass(device, renderPassCreateInfo, null, out var renderPass).EnsureSuccess();
        VulkanUtils.SetDebugObjectName(ObjectType.RenderPass, Options.DebugName, renderPass.Handle);
        RenderPass = renderPass;


        // TODO


        var framebufferCreateInfo = new FramebufferCreateInfo(StructureType.FramebufferCreateInfo) {
            // TODO
            RenderPass = renderPass, Width = (uint)size.Width, Height = (uint)size.Height, Layers = 1
        };

        vk.CreateFramebuffer(device, framebufferCreateInfo, null, out var framebuffer).EnsureSuccess();
        VulkanUtils.SetDebugObjectName(ObjectType.Framebuffer, Options.DebugName, framebuffer.Handle);
        this.framebuffer = framebuffer;
    }

    AttachmentLoadOp GetAttachmentLoadOp(FramebufferTextureOptions textureOptions) {
        if (textureOptions.LoadOp == Abstractions.Rendering.AttachmentLoadOp.Inherit) {
            if (textureOptions.Format.IsDepthFormat()) {
                return Options.ClearDepthOnLoad ? AttachmentLoadOp.Clear : AttachmentLoadOp.Load;
            }

            return Options.ClearColorOnLoad ? AttachmentLoadOp.Clear : AttachmentLoadOp.Load;
        }

        return textureOptions.LoadOp == Abstractions.Rendering.AttachmentLoadOp.Clear
            ? AttachmentLoadOp.Clear
            : AttachmentLoadOp.Load;
    }
}
