using System.Drawing;
using Vixen.Core.Common;
using Vixen.Platform.Common.Rendering;
using Vixen.Platform.Internal;

namespace Vixen.Core.General;

public class Window {
    internal readonly IWindow Handle;

    public Window(Action<WindowOptions>? configureOptions = null) {
        var options = new WindowOptions { Title = "Rin Engine", Size = new(1600, 900), Decorated = true, VSync = true };

        configureOptions?.Invoke(options);

        Handle = ObjectFactory.CreateWindow(options, InputContainer.inputManager);
        Handle.Closing += () => Closing?.Invoke();
        Handle.Resize += size => Resize?.Invoke(size);
    }

    public event Action? Closing;
    public event Action<Size>? Resize;
}
