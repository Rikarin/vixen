using Silk.NET.Vulkan;
using Vixen.Platform.Common.Rendering;

namespace Vixen.Platform.Vulkan;

sealed class VulkanRenderPass : IRenderPass {
    readonly DescriptorSetManager descriptorSetManager;

    public RenderPassOptions Options { get; }
    public bool IsBaked => throw new NotImplementedException();
    public IPipeline Pipeline => Options.Pipeline;
    public IFramebuffer TargetFramebuffer => Options.Pipeline.Options.TargetFramebuffer;
    public int FirstSetIndex => descriptorSetManager.FirstSetIndex;
    public bool HasDescriptorSets => descriptorSetManager.HasDescriptorSets;

    public VulkanRenderPass(RenderPassOptions options) {
        Options = options;

        descriptorSetManager = new(
            new() {
                DebugName = options.DebugName, Shader = options.Pipeline.Options.Shader as VulkanShader, StartSet = 1
            }
        );
    }

    public IImage2D GetOutput(int index) {
        var framebuffer = Options.Pipeline.Options.TargetFramebuffer;
        if (index > framebuffer.ColorAttachmentCount + 1) {
            throw new ArgumentOutOfRangeException();
        }

        if (index < framebuffer.ColorAttachmentCount) {
            return framebuffer.GetImage(index);
        }

        return framebuffer.DepthImage!;
    }

    public IImage GetDepthOutput() {
        var framebuffer = Options.Pipeline.Options.TargetFramebuffer;
        if (framebuffer.DepthImage != null) {
            return framebuffer.DepthImage;
        }

        throw new ArgumentOutOfRangeException();
    }

    public List<DescriptorSet> GetDescriptorSets(int frameIndex) {
        if (frameIndex > 0 && descriptorSetManager.DescriptorSets.Count == 1) {
            return descriptorSetManager.DescriptorSets[0];
        }

        return descriptorSetManager.DescriptorSets[frameIndex];
    }

    public void SetInput(string name, IUniformBufferSet uniformBufferSet) {
        descriptorSetManager.SetInput(name, uniformBufferSet);
    }

    public void SetInput(string name, IUniformBuffer uniformBuffer) {
        descriptorSetManager.SetInput(name, uniformBuffer);
    }

    public void SetInput(string name, IStorageBufferSet storageBufferSet) {
        descriptorSetManager.SetInput(name, storageBufferSet);
    }

    public void SetInput(string name, IStorageBuffer storageBuffer) {
        descriptorSetManager.SetInput(name, storageBuffer);
    }

    public void SetInput(string name, ITexture2D texture) {
        descriptorSetManager.SetInput(name, texture);
    }

    public void SetInput(string name, ITextureCube textureCube) {
        descriptorSetManager.SetInput(name, textureCube);
    }

    public void SetInput(string name, IImage2D image) {
        descriptorSetManager.SetInput(name, image);
    }

    public bool Validate() => descriptorSetManager.Validate();
    public void Bake() => descriptorSetManager.Bake();
    public void Prepare() => descriptorSetManager.InvalidateAndUpdate();
}
