using Rin.Core.Abstractions;
using System.Drawing;
using System.Numerics;

namespace Rin.Platform.Abstractions.Rendering;

public interface IWindow {
    Size Size { get; }
    Vector2 MousePosition { get; }
    RendererContext RendererContext { get; }
    ISwapchain Swapchain { get; }

    bool GetKey(Key key);
    bool GetKeyDown(Key key);
    bool GetKeyUp(Key key);
    Vector2 GetMouseAxis();

    bool GetMouseButton(MouseButton mouseButton);
    bool GetMouseButtonDown(MouseButton mouseButton);
    bool GetMouseButtonUp(MouseButton mouseButton);
    event Action? Closing;
    event Action<Size>? Resize;
}
