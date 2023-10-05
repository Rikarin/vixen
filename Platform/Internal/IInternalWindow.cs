using Rin.Core.Abstractions;
using Rin.Platform.Rendering;
using System.Numerics;

namespace Rin.Platform.Internal;

public interface IInternalWindow {
    Vector2 MousePosition { get; }
    RendererContext RendererContext { get; }
    ISwapchain Swapchain { get; }

    IInternalGuiRenderer CreateGuiRenderer();
    void Run();

    bool GetKey(Key key);
    bool GetKeyDown(Key key);
    bool GetKeyUp(Key key);
    Vector2 GetMouseAxis();
    bool GetMouseButtonDown(MouseButton mouseButton);
    bool GetMouseButtonUp(MouseButton mouseButton);
    event Action? Load;
    public event Action? Closing;
    event Action<float>? Render;
}
