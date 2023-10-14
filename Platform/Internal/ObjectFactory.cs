using Rin.Core.Abstractions;
using Rin.Platform.Abstractions.Rendering;
using Rin.Platform.Silk;
using Rin.Platform.Vulkan;
using Rin.Rendering;

namespace Rin.Platform.Internal;

public static class ObjectFactory {
    public static IWindow CreateWindow(WindowOptions options) => new SilkWindow(options);

    public static RendererContext CreateRendererContext() => new VulkanContext();

    public static IIndexBuffer CreateIndexBuffer(ReadOnlySpan<byte> data) {
        switch (Renderer.CurrentApi) {
            case RenderingApi.Vulkan: return new VulkanIndexBuffer(data);
            default: throw new ArgumentOutOfRangeException();
        }
    }

    public static IStorageBuffer CreateStorageBuffer(StorageBufferOptions options, int size) {
        switch (Renderer.CurrentApi) {
            case RenderingApi.Vulkan: return new VulkanStorageBuffer(options, size);
            default: throw new ArgumentOutOfRangeException();
        }
    }

    public static IStorageBufferSet CreateStorageBufferSet(
        StorageBufferOptions options,
        int size,
        int framesInFlight = 0
    ) {
        switch (Renderer.CurrentApi) {
            case RenderingApi.Vulkan: return new VulkanStorageBufferSet(options, size, framesInFlight);
            default: throw new ArgumentOutOfRangeException();
        }
    }

    public static IUniformBuffer CreateUniformBuffer(int size) {
        switch (Renderer.CurrentApi) {
            case RenderingApi.Vulkan: return new VulkanUniformBuffer(size);
            default: throw new ArgumentOutOfRangeException();
        }
    }

    public static IUniformBufferSet CreateUniformBufferSet(int size, int framesInFlight = 0) {
        switch (Renderer.CurrentApi) {
            case RenderingApi.Vulkan: return new VulkanUniformBufferSet(size, framesInFlight);
            default: throw new ArgumentOutOfRangeException();
        }
    }

    // TODO: "usage" parameter is not used anywhere
    public static IVertexBuffer CreateVertexBuffer(int size, VertexBufferUsage usage = VertexBufferUsage.Dynamic) {
        switch (Renderer.CurrentApi) {
            case RenderingApi.Vulkan: return new VulkanVertexBuffer(size, usage);
            default: throw new ArgumentOutOfRangeException();
        }
    }

    // TODO: "usage" parameter is not used anywhere
    public static IVertexBuffer CreateVertexBuffer(
        ReadOnlySpan<byte> data,
        VertexBufferUsage usage = VertexBufferUsage.Static
    ) {
        switch (Renderer.CurrentApi) {
            case RenderingApi.Vulkan: return new VulkanVertexBuffer(data, usage);
            default: throw new ArgumentOutOfRangeException();
        }
    }

    public static IFramebuffer CreateFramebuffer(FramebufferOptions options) {
        switch (Renderer.CurrentApi) {
            case RenderingApi.Vulkan: return new VulkanFramebuffer(options);
            default: throw new ArgumentOutOfRangeException();
        }
    }

    public static IImage2D CreateImage2D(ImageOptions options) {
        switch (Renderer.CurrentApi) {
            case RenderingApi.Vulkan: return new VulkanImage2D(options);
            default: throw new ArgumentOutOfRangeException();
        }
    }

    public static IImageView CreateImageView(ImageViewOptions options) {
        switch (Renderer.CurrentApi) {
            case RenderingApi.Vulkan: return new VulkanImageView(options);
            default: throw new ArgumentOutOfRangeException();
        }
    }
    
    public static IRenderPass CreateRenderPass(RenderPassOptions options) {
        switch (Renderer.CurrentApi) {
            case RenderingApi.Vulkan: return new VulkanRenderPass(options);
            default: throw new ArgumentOutOfRangeException();
        }
    }
    
    public static IPipeline CreatePipeline(PipelineOptions options) {
        switch (Renderer.CurrentApi) {
            case RenderingApi.Vulkan: return new VulkanPipeline(options);
            default: throw new ArgumentOutOfRangeException();
        }
    }
    
    public static IRenderCommandBuffer CreateRenderCommandBuffer(int? count = null, string? name = null) {
        throw new NotImplementedException();
        switch (Renderer.CurrentApi) {
            // case RendererApi.Api.Vulkan: return new VulkanRenderCommandBuffer(); // TODO: Pass arguments
            default: throw new ArgumentOutOfRangeException();
        }
    }
    
    public static IRenderCommandBuffer CreateRenderCommandBufferFromSwapChain(string? name = null) {
        switch (Renderer.CurrentApi) {
            case RenderingApi.Vulkan: return new VulkanRenderCommandBuffer(name ?? "unknown", true); // TODO: Pass arguments
            default: throw new ArgumentOutOfRangeException();
        }
    }
    
    public static IMaterial CreateMaterial(IShader shader, string name) {
        switch (Renderer.CurrentApi) {
            case RenderingApi.Vulkan: return new VulkanMaterial(shader, name);
            default: throw new ArgumentOutOfRangeException();
        }
    }
}
