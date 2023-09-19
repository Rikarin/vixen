using Rin.Platform.Internal;
using Rin.Platform.Silk;

namespace Rin.Core.General;

public class Window {
    IWindow handle;

    public Window() {
        handle = new SilkWindow();
    }

    public void Run() {
        handle.Run();
    }
}
