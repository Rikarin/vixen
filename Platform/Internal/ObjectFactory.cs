using Rin.Core.Abstractions;
using Rin.Platform.Silk;
using Rin.Platform.Vulkan;

namespace Rin.Platform.Internal; 

static class ObjectFactory {
    public static IInternalWindow CreateWindow(WindowOptions options) {
        return new SilkWindow(options);
    }

    public static RendererContext CreateRendererContext() {
        return new VulkanContext();
    }
}
