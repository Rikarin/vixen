using Rin.Core.Abstractions;
using Rin.Platform.Rendering;
using Rin.Platform.Silk;
using Rin.Platform.Vulkan;

namespace Rin.Platform.Internal;

static class ObjectFactory {
    public static IInternalWindow CreateWindow(WindowOptions options) => new SilkWindow(options);

    public static RendererContext CreateRendererContext() => new VulkanContext();

    public static IIndexBuffer CreateIndexBuffer(ReadOnlySpan<byte> data) {
        switch (RendererApi.CurrentApi) {
            case RendererApi.Api.Vulkan: return new VulkanIndexBuffer(data);
            default: throw new ArgumentOutOfRangeException();
        }
    }

    public static IStorageBuffer CreateStorageBuffer(StorageBufferOptions options, int size) {
        switch (RendererApi.CurrentApi) {
            case RendererApi.Api.Vulkan: return new VulkanStorageBuffer(options, size);
            default: throw new ArgumentOutOfRangeException();
        }
    }

    public static IStorageBufferSet CreateStorageBufferSet(
        StorageBufferOptions options,
        int size,
        int framesInFlight = 0
    ) {
        switch (RendererApi.CurrentApi) {
            case RendererApi.Api.Vulkan: return new VulkanStorageBufferSet(options, size, framesInFlight);
            default: throw new ArgumentOutOfRangeException();
        }
    }

    public static IUniformBuffer CreateUniformBuffer(int size) {
        switch (RendererApi.CurrentApi) {
            case RendererApi.Api.Vulkan: return new VulkanUniformBuffer(size);
            default: throw new ArgumentOutOfRangeException();
        }
    }

    public static IUniformBufferSet CreateUniformBufferSet(int size, int framesInFlight = 0) {
        switch (RendererApi.CurrentApi) {
            case RendererApi.Api.Vulkan: return new VulkanUniformBufferSet(size, framesInFlight);
            default: throw new ArgumentOutOfRangeException();
        }
    }

    // TODO: "usage" parameter is not used anywhere
    public static IVertexBuffer CreateVertexBuffer(int size, VertexBufferUsage usage = VertexBufferUsage.Dynamic) {
        switch (RendererApi.CurrentApi) {
            case RendererApi.Api.Vulkan: return new VulkanVertexBuffer(size, usage);
            default: throw new ArgumentOutOfRangeException();
        }
    }

    // TODO: "usage" parameter is not used anywhere
    public static IVertexBuffer CreateVertexBuffer(
        ReadOnlySpan<byte> data,
        VertexBufferUsage usage = VertexBufferUsage.Static
    ) {
        switch (RendererApi.CurrentApi) {
            case RendererApi.Api.Vulkan: return new VulkanVertexBuffer(data, usage);
            default: throw new ArgumentOutOfRangeException();
        }
    }

    public static IFramebuffer CreateFramebuffer(FramebufferOptions options) {
        switch (RendererApi.CurrentApi) {
            case RendererApi.Api.Vulkan: return new VulkanFramebuffer(options);
            default: throw new ArgumentOutOfRangeException();
        }
    }

    public static IImage2D CreateImage2D(ImageOptions options) {
        switch (RendererApi.CurrentApi) {
            case RendererApi.Api.Vulkan: return new VulkanImage2D(options);
            default: throw new ArgumentOutOfRangeException();
        }
    }

    public static IImageView CreateImageView(ImageViewOptions options) {
        switch (RendererApi.CurrentApi) {
            case RendererApi.Api.Vulkan: return new VulkanImageView(options);
            default: throw new ArgumentOutOfRangeException();
        }
    }
}
