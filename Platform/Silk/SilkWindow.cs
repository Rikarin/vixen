using Serilog;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Rin.Core")]
namespace Rin.Platform.Silk;

sealed class SilkWindow : Internal.IWindow {
    internal static SilkWindow MainWindow;
    IWindow window = null!;

    internal GL Gl { get; private set; }

    internal SilkWindow() {
        MainWindow = this;
        Initialize();
    }

    public void Run() {
        window.Run();
    }

    void Initialize() {
        var options = WindowOptions.Default with { };
        window = global::Silk.NET.Windowing.Window.Create(options);

        window.Load += OnLoad;
        window.Update += OnUpdate;
        window.Render += OnRender;
        window.Closing += OnClosing;
    }

    void OnRender(double obj) { }

    void OnUpdate(double obj) { }

    void OnLoad() {
        var input = window.CreateInput();

        foreach (var keyboard in input.Keyboards) {
            keyboard.KeyDown += KeyDown;
            keyboard.KeyUp += KeyUp;
        }

        Gl = GL.GetApi(window.GLContext);
        Gl.Enable(EnableCap.DepthTest);
    }

    void OnClosing() { }

    void KeyDown(IKeyboard keyboard, Key key, int index) {
        Log.Information("Key {Key} pressed", key);
        if (key == Key.Escape) {
            window.Close();
        }
    }

    void KeyUp(IKeyboard keyboard, Key key, int index) {
        Log.Information("Key {Key} released", key);
    }
}
