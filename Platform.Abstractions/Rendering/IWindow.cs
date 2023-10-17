using Rin.Core.Abstractions;
using System.Drawing;
using System.Numerics;

namespace Rin.Platform.Abstractions.Rendering;

public interface IWindow {
    Size Size { get; }
    RendererContext RendererContext { get; }
    ISwapchain Swapchain { get; }

    event Action? Closing;
    event Action<Size>? Resize;
}
