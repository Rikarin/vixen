using Rin.Core.Abstractions;
using System.Drawing;

namespace Rin.Platform.Abstractions.Rendering; 

public interface ISwapchain : ICurrentBufferIndexAccessor {
    Size Size { get; }
    
    void BeginFrame();
    void Present();
}
