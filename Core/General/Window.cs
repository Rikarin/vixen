using Rin.Platform.Internal;
using Rin.Platform.Silk;

namespace Rin.Core.General;

public class Window {
    internal readonly IWindow handle;

    public event Action? Load;
    public event Action<float>? Render;

    public Window() {
        handle = new SilkWindow();
        handle.Load += () => Load?.Invoke();
        handle.Render += deltaTime => Render?.Invoke(deltaTime);
    }

    public void Run() {
        handle.Run();
    }
}
