using Rin.Core.Abstractions;
using System.Drawing;

namespace Rin.Platform.Abstractions.Rendering; 

public interface ISwapchain : ICurrentBufferIndexAccessor {
    Size Size { get; }

    void OnResize(Size size);
    void BeginFrame();
    void Present();
}
