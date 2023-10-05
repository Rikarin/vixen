using Rin.Core.Abstractions;
using Rin.Platform.Internal;
using Rin.Platform.Rendering;
using Rin.Platform.Silk;
using Serilog;
using Silk.NET.Vulkan;
using System.Drawing;

namespace Rin.Platform.Vulkan;

public sealed class VulkanFramebuffer : IFramebuffer, IDisposable {

    // RenderPass renderPass;
    Framebuffer? framebuffer;
    readonly Size size;

    readonly List<IImage2D> attachmentImages = new();

    public FramebufferOptions Options { get; }
    public IImage2D? DepthImage { get; private set; }
    public RenderPass RenderPass { get; private set; }

    public int ColorAttachmentCount => Options.IsSwapChainTarget ? 1 : attachmentImages.Count;

    // TODO: others

    public VulkanFramebuffer(FramebufferOptions options) {
        this.Options = options;

        if (options.Size == null) {
            throw new NotImplementedException();
        }

        size = new(
            (int)(options.Size.Value.Width * options.Scale),
            (int)(options.Size.Value.Height * options.Scale)
        );

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

    public void Dispose() {
        Release();
    }

    // public void Invalidate() => Renderer.Submit(Invalidate_RT);
    public IImage2D GetImage(int index) => attachmentImages[index];


    void Resize(Size newSize, bool forceRecreate) {
        if (!forceRecreate && newSize == size) {
            return;
        }
        
        Renderer.Submit(
            () => {
                if (!Options.IsSwapChainTarget) {
                    Invalidate_RT();
                } else {
                    var vkSwapchain = SilkWindow.MainWindow.Swapchain as VulkanSwapChain;
                    RenderPass = vkSwapchain.RenderPass.Value;
                }
            });
        // TODO
    }

    unsafe void Release() {
        if (framebuffer != null) {
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
    }

    unsafe void Invalidate_RT() {
        var device = VulkanContext.CurrentDevice.VkLogicalDevice;
        var vk = VulkanContext.Vulkan;
        
        Release();
        
        // TODO: stuff
        
        
        // Create Render Pass
        var renderPassCreateInfo = new RenderPassCreateInfo(StructureType.RenderPassCreateInfo) {
            // TODO
            // AttachmentCount = 0
        };

        vk.CreateRenderPass(device, renderPassCreateInfo, null, out var renderPass);
        VulkanUtils.SetDebugObjectName(ObjectType.RenderPass, Options.DebugName, renderPass.Handle);
        RenderPass = renderPass;


        // TODO



        var framebufferCreateInfo = new FramebufferCreateInfo(StructureType.FramebufferCreateInfo) {
            // TODO
            RenderPass = renderPass
        };

        vk.CreateFramebuffer(device, framebufferCreateInfo, null, out var framebuffer);
        VulkanUtils.SetDebugObjectName(ObjectType.Framebuffer, Options.DebugName, framebuffer.Handle);
        this.framebuffer = framebuffer;
    }
}
