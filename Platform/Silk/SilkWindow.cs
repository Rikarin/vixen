using Rin.Platform.Internal;
using Serilog;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System.Numerics;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Rin.Core")]
[assembly: InternalsVisibleTo("Rin.Editor")]

namespace Rin.Platform.Silk;

sealed class SilkWindow : IInternalWindow {
    internal static SilkWindow MainWindow;
    internal IWindow silkWindow = null!;
    internal IInputContext input;

    // TODO: this needs to be fixed
    // I think user can press multiple keys during one frame
    Key? keyDown;
    Key? keyUp;
    MouseButton? mouseButtonDown;
    MouseButton? mouseButtonUp;
    Vector2 mouseAxis = Vector2.Zero;

    readonly bool[] keyPressed = new bool[(int)Key.Menu + 1];
    readonly bool[] mouseButtonPressed = new bool[(int)MouseButton.Button12 + 1];

    public Vector2 MousePosition { get; private set; } = Vector2.Zero;

    internal GL Gl { get; private set; }

    internal SilkWindow() {
        MainWindow = this;
        Initialize();
    }

    public IInternalGuiRenderer CreateGuiRenderer() => new SilkImGuiRenderer(new(Gl, silkWindow, input));

    public void Run() {
        silkWindow.Run();
    }

    public bool GetKey(Core.Abstractions.Key key) => keyPressed[(int)key];
    public bool GetKeyDown(Core.Abstractions.Key key) => keyDown.HasValue && (int)keyDown.Value == (int)key;
    public bool GetKeyUp(Core.Abstractions.Key key) => keyUp.HasValue && (int)keyUp.Value == (int)key;

    public Vector2 GetMouseAxis() => mouseAxis;

    public bool GetMouseButtonDown(Core.Abstractions.MouseButton mouseButton) =>
        mouseButtonDown.HasValue && (int)mouseButtonDown.Value == (int)mouseButton;

    public bool GetMouseButtonUp(Core.Abstractions.MouseButton mouseButton) =>
        mouseButtonUp.HasValue && (int)mouseButtonUp.Value == (int)mouseButton;

    void Initialize() {
        var options = WindowOptions.Default with { };
        silkWindow = Window.Create(options);

        silkWindow.Load += OnLoad;
        silkWindow.Render += OnRender;
        silkWindow.Closing += OnClosing;
        silkWindow.FramebufferResize += vector2D => Gl.Viewport(vector2D);
    }

    void OnClosing() {
        Closing?.Invoke();
    }

    void OnRender(double deltaTime) {
        Render?.Invoke((float)deltaTime);
        keyDown = null;
        keyUp = null;
        mouseAxis = Vector2.Zero;
        mouseButtonDown = null;
        mouseButtonUp = null;
    }

    void OnLoad() {
        Gl = silkWindow.CreateOpenGL();
        input = silkWindow.CreateInput();

        foreach (var keyboard in input.Keyboards) {
            keyboard.KeyDown += KeyDown;
            keyboard.KeyUp += KeyUp;
        }

        foreach (var mouse in input.Mice) {
            mouse.MouseDown += OnMouseDown;
            mouse.MouseUp += OnMouseUp;
            mouse.MouseMove += OnMouseMove;
            // click, scroll, double click??
        }

        Log.Information("OpenGL Context initialized");
        Load?.Invoke();
    }

    void OnMouseMove(IMouse arg1, Vector2 arg2) {
        mouseAxis = arg2 - MousePosition;
        MousePosition = arg2;
    }

    void OnMouseUp(IMouse arg1, MouseButton arg2) {
        mouseButtonPressed[(int)arg2] = false;
        mouseButtonUp = arg2;
    }

    void OnMouseDown(IMouse arg1, MouseButton arg2) {
        mouseButtonPressed[(int)arg2] = true;
        mouseButtonDown = arg2;
    }

    void KeyDown(IKeyboard keyboard, Key key, int index) {
        keyPressed[(int)key] = true;
        keyDown = key;

        if (key == Key.Escape) {
            silkWindow.Close();
        }
    }

    void KeyUp(IKeyboard keyboard, Key key, int index) {
        keyPressed[(int)key] = false;
        keyUp = key;
    }

    public event Action? Load;
    public event Action? Closing;
    public event Action<float>? Render;
}
