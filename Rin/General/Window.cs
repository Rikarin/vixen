using Editor.Platform.Silk;

namespace Editor.General;

public abstract class Window {
    public abstract void Run();

    public static Window CreateWindow() => new SilkWindow();
}
