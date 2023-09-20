using Rin.Platform.Internal;
using Rin.Platform.Silk;

namespace Rin.Core.General;

public class Window {
    internal readonly IWindow handle;

    public event Action? Load;
    public event Action? Render;

    public Window() {
        handle = new SilkWindow();
        handle.Load += () => Load?.Invoke();
        handle.Render += () => Render?.Invoke();
    }

    public void Run() {
        handle.Run();
    }
}
