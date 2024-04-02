using System.Drawing;
using Vixen.Core.Common;

namespace Vixen.Platform.Common.Rendering; 

public interface ISwapchain : ICurrentBufferIndexAccessor {
    Size Size { get; }

    void OnResize(Size size);
    void BeginFrame();
    void Present();
}
