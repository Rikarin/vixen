using System.Drawing;
using Vixen.Core.Common;

namespace Vixen.Platform.Common.Rendering;

public interface IWindow {
    Size Size { get; }
    RendererContext RendererContext { get; }
    ISwapchain Swapchain { get; }

    event Action? Closing;
    event Action<Size>? Resize;
}
