using Rin.Core.Abstractions;

namespace Rin.Platform.Rendering; 

public interface ISwapchain : ICurrentBufferIndexAccessor {
    void BeginFrame();
    void Present();
}
