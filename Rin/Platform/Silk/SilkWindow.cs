using Silk.NET.Windowing;
using Window = Editor.General.Window;

namespace Editor.Platform.Silk;

public sealed class SilkWindow : Window {
    IWindow window = null!;

    internal SilkWindow() {
        Initialize();
    }

    public override void Run() {
        window.Run();
    }

    void Initialize() {
        var options = WindowOptions.Default with { };
        window = global::Silk.NET.Windowing.Window.Create(options);
    }
}
