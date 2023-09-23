using Rin.Platform.Internal;

namespace Rin.Core.General;

public class Window {
    internal readonly IInternalWindow Handle;

    public Window() {
        Handle = ObjectFactory.CreateWindow();
        Handle.Load += () => Load?.Invoke();
        Handle.Closing += () => Closing?.Invoke();
        Handle.Render += deltaTime => Render?.Invoke(deltaTime);
    }

    public void Run() {
        Handle.Run();
    }

    public event Action? Load;
    public event Action? Closing;
    public event Action<float>? Render;
}
