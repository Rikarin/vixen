using Rin.Core.Abstractions;
using Rin.Platform.Internal;

namespace Rin.Core.General;

public class Window {
    internal readonly IInternalWindow Handle;

    public Window(Action<WindowOptions>? configureOptions = null) {
        var options = new WindowOptions { Title = "Rin", Size = new(1600, 900), Decorated = true, VSync = true };

        configureOptions?.Invoke(options);

        Handle = ObjectFactory.CreateWindow(options);
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

    public void Test_InvokeLoad() {
        Load?.Invoke();
    }
}
