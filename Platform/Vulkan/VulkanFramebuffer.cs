using Rin.Core.Abstractions;
using Rin.Platform.Abstractions.Rendering;
using Rin.Platform.Internal;
using Rin.Platform.Silk;
using Rin.Rendering;
using Silk.NET.Vulkan;
using System.Diagnostics;
using System.Drawing;
using AttachmentLoadOp = Silk.NET.Vulkan.AttachmentLoadOp;

namespace Rin.Platform.Vulkan;

sealed class VulkanFramebuffer : IFramebuffer, IDisposable {
    // Not sure if use list or dictionary
    readonly List<IImage2D?> attachmentImages = new();

    public List<ClearValue> ClearValues { get; } = new();
    public FramebufferOptions Options { get; }
    public Size Size { get; private set; }
    public IImage2D? DepthImage { get; private set; }
    public int ColorAttachmentCount => Options.IsSwapChainTarget ? 1 : attachmentImages.Count;

    internal RenderPass VkRenderPass { get; private set; }
    internal Framebuffer? VkFramebuffer { get; private set; }

    public VulkanFramebuffer(FramebufferOptions options) {
        Options = options;

        if (options.Size == null) {
            Size = SilkWindow.MainWindow.Size;
        } else {
            Size = options.Size.Value.Multiply(options.Scale);
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
                        attachmentImages.Add(null);
                    }
                } else if (attachment.Format.IsDepthFormat()) {
                    var imageOptions = new ImageOptions {
                        Format = attachment.Format,
                        Usage = ImageUsage.Attachment,
                        Transfer = options.Transfer,
                        Size = Size,
                        DebugName = $"{options.DebugName ?? "Unknown FrameBuffer"}-DepthAttachment{attachmentIndex}"
                    };
                    DepthImage = ObjectFactory.CreateImage2D(imageOptions);
                } else {
                    var imageOptions = new ImageOptions {
                        Format = attachment.Format,
                        Usage = ImageUsage.Attachment,
                        Transfer = options.Transfer,
                        Size = Size,
                        DebugName = $"{options.DebugName ?? "Unknown FrameBuffer"}-ColorAttachment{attachmentIndex}"
                    };
                    attachmentImages.Add(ObjectFactory.CreateImage2D(imageOptions));
                }

                attachmentIndex++;
            }
        }

        Resize(Size, true);
    }

    public void Dispose() => Release();
    public IImage2D GetImage(int index) => attachmentImages[index];

    void Resize(Size newSize, bool forceRecreate) {
        if (!forceRecreate && newSize == Size) {
            return;
        }

        Renderer.Submit(
            () => {
                Size = newSize.Multiply(Options.Scale);
                if (!Options.IsSwapChainTarget) {
                    Invalidate_RT();
                } else {
                    var vkSwapchain = SilkWindow.MainWindow.Swapchain as VulkanSwapChain; // TODO
                    VkRenderPass = vkSwapchain.RenderPass.Value;

                    ClearValues.Clear();
                    ClearValues.Add(new(new(0, 0, 0, 1)));
                }
            }
        );
    }

    unsafe void Release() {
        if (VkFramebuffer == null) {
            return;
        }

        Renderer.SubmitDisposal(
            () => VulkanContext.Vulkan.DestroyFramebuffer(
                VulkanContext.CurrentDevice.VkLogicalDevice,
                VkFramebuffer.Value,
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
        var vk = VulkanContext.Vulkan;
        var device = VulkanContext.CurrentDevice.VkLogicalDevice;

        Release();

        var attachmentDescriptions = new List<AttachmentDescription>();
        var colorAttachmentReferences = new List<AttachmentReference>();
        var depthAttachmentReference = new AttachmentReference();

        ClearValues.Clear();
        var createImages = attachmentImages.Count == 0;

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
                } else if (
                    Options.ExistingImages != null
                    && Options.ExistingImages.TryGetValue(attachmentIndex, out var existingImage)
                ) {
                    DepthImage = existingImage;
                } else {
                    var vkDepthImage = DepthImage as VulkanImage2D;
                    vkDepthImage.Options.Size = Size;
                    vkDepthImage.Invalidate_RT();
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
                        FinalLayout = ImageLayout.DepthStencilReadOnlyOptimal
                    }
                );

                depthAttachmentReference = new(
                    (uint)attachmentIndex,
                    ImageLayout.DepthStencilAttachmentOptimal
                );
                ClearValues.Add(new() { DepthStencil = new(Options.DepthClearValue, 0) });
            } else {
                if (Options.ExistingFramebuffer != null) {
                    var existingImage = (Options.ExistingFramebuffer as VulkanFramebuffer).GetImage(attachmentIndex);
                    attachmentImages.Add(existingImage);
                } else if (
                    Options.ExistingImages != null
                    && Options.ExistingImages.TryGetValue(attachmentIndex, out var existingImage)
                ) {
                    Trace.Assert(
                        !existingImage.Options.Format.IsDepthFormat(),
                        "Trying to attach depth image as color attachment"
                    );
                    attachmentImages[attachmentIndex] = existingImage;
                } else {
                    if (createImages) {
                        attachmentImages.Add(
                            (VulkanImage2D)ObjectFactory.CreateImage2D(
                                new() {
                                    Format = attachment.Format,
                                    Usage = ImageUsage.Attachment,
                                    Transfer = Options.Transfer,
                                    Size = Size
                                }
                            )
                        );
                    } else {
                        var image = attachmentImages[attachmentIndex] as VulkanImage2D;
                        image.Options.Size = Size;

                        if (image.Options.Layers == 1) {
                            image.Invalidate_RT();
                        } else if (attachmentIndex == 0 && Options.ExistingImageLayers[0] == 0) {
                            image.Invalidate_RT();
                            image.CreatePerSpecificLayerImageViews_RT(Options.ExistingImageLayers);
                        } else if (attachmentIndex == 0) {
                            image.CreatePerSpecificLayerImageViews_RT(Options.ExistingImageLayers);
                        }
                    }
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
                                : ImageLayout.ShaderReadOnlyOptimal,
                        FinalLayout = ImageLayout.ShaderReadOnlyOptimal
                    }
                );

                var c = Options.ClearColor;
                ClearValues.Add(new() { Color = new(c.R, c.G, c.B, c.A) });
                colorAttachmentReferences.Add(new((uint)attachmentIndex, ImageLayout.ColorAttachmentOptimal));
            }
        }

        using var colorAttachmentsMemoryHandle =
            new Memory<AttachmentReference>(colorAttachmentReferences.ToArray()).Pin();
        var subpassDescription = new SubpassDescription {
            PipelineBindPoint = PipelineBindPoint.Graphics,
            ColorAttachmentCount = (uint)colorAttachmentReferences.Count,
            PColorAttachments = (AttachmentReference*)colorAttachmentsMemoryHandle.Pointer
        };

        if (DepthImage != null) {
            subpassDescription.PDepthStencilAttachment = &depthAttachmentReference;
        }

        var dependencies = new List<SubpassDependency>();
        if (attachmentImages.Count != 0) {
            dependencies.Add(
                new() {
                    SrcSubpass = ~0U,
                    SrcStageMask = PipelineStageFlags.FragmentShaderBit,
                    DstStageMask = PipelineStageFlags.ColorAttachmentOutputBit,
                    SrcAccessMask = AccessFlags.ShaderReadBit,
                    DstAccessMask = AccessFlags.ColorAttachmentWriteBit,
                    DependencyFlags = DependencyFlags.ByRegionBit
                }
            );

            dependencies.Add(
                new() {
                    DstSubpass = ~0U,
                    SrcStageMask = PipelineStageFlags.ColorAttachmentOutputBit,
                    DstStageMask = PipelineStageFlags.FragmentShaderBit,
                    SrcAccessMask = AccessFlags.ColorAttachmentWriteBit,
                    DstAccessMask = AccessFlags.ShaderReadBit,
                    DependencyFlags = DependencyFlags.ByRegionBit
                }
            );
        }

        if (DepthImage != null) {
            dependencies.Add(
                new() {
                    SrcSubpass = ~0U,
                    SrcStageMask = PipelineStageFlags.FragmentShaderBit,
                    DstStageMask = PipelineStageFlags.EarlyFragmentTestsBit,
                    SrcAccessMask = AccessFlags.ShaderReadBit,
                    DstAccessMask = AccessFlags.DepthStencilAttachmentWriteBit,
                    DependencyFlags = DependencyFlags.ByRegionBit
                }
            );

            dependencies.Add(
                new() {
                    DstSubpass = ~0U,
                    SrcStageMask = PipelineStageFlags.LateFragmentTestsBit,
                    DstStageMask = PipelineStageFlags.FragmentShaderBit,
                    SrcAccessMask = AccessFlags.DepthStencilAttachmentWriteBit,
                    DstAccessMask = AccessFlags.ShaderReadBit,
                    DependencyFlags = DependencyFlags.ByRegionBit
                }
            );
        }

        // Create Render Pass
        using var attachmentDescriptorsMemoryHandle =
            new Memory<AttachmentDescription>(attachmentDescriptions.ToArray()).Pin();
        using var dependenciesMemoryHandle = new Memory<SubpassDependency>(dependencies.ToArray()).Pin();
        var renderPassCreateInfo = new RenderPassCreateInfo(StructureType.RenderPassCreateInfo) {
            PAttachments = (AttachmentDescription*)attachmentDescriptorsMemoryHandle.Pointer,
            AttachmentCount = (uint)attachmentDescriptions.Count,
            SubpassCount = 1,
            PSubpasses = &subpassDescription,
            DependencyCount = (uint)dependencies.Count,
            PDependencies = (SubpassDependency*)dependenciesMemoryHandle.Pointer
        };

        vk.CreateRenderPass(device, renderPassCreateInfo, null, out var renderPass).EnsureSuccess();
        VulkanUtils.SetDebugObjectName(ObjectType.RenderPass, Options.DebugName, renderPass.Handle);
        VkRenderPass = renderPass;

        // Attachments
        var attachments = new List<ImageView>();
        for (var i = 0; i < attachmentImages.Count; i++) {
            var vkImage = attachmentImages[i] as VulkanImage2D;
            if (vkImage.Options.Layers > 1) {
                attachments.Add(vkImage.GetLayerImageView(Options.ExistingImageLayers[i]));
            } else {
                attachments.Add(vkImage.ImageInfo.ImageView.Value);
            }
        }

        if (DepthImage != null) {
            var vkImage = DepthImage as VulkanImage2D;
            if (Options.ExistingImage != null && vkImage.Options.Layers > 1) {
                attachments.Add(vkImage.GetLayerImageView(Options.ExistingImageLayers[0]));
            } else {
                attachments.Add(vkImage.ImageInfo.ImageView.Value);
            }
        }

        var attachmentsMemoryHandle = new Memory<ImageView>(attachments.ToArray()).Pin();
        var framebufferCreateInfo = new FramebufferCreateInfo(StructureType.FramebufferCreateInfo) {
            PAttachments = (ImageView*)attachmentsMemoryHandle.Pointer,
            AttachmentCount = (uint)attachments.Count,
            RenderPass = renderPass,
            Width = (uint)Size.Width,
            Height = (uint)Size.Height,
            Layers = 1
        };

        vk.CreateFramebuffer(device, framebufferCreateInfo, null, out var framebuffer).EnsureSuccess();
        VulkanUtils.SetDebugObjectName(ObjectType.Framebuffer, Options.DebugName, framebuffer.Handle);
        VkFramebuffer = framebuffer;
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
