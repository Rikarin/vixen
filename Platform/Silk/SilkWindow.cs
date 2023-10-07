using Rin.Core.Abstractions;
using Rin.Platform.Abstractions.Rendering;
using Rin.Platform.Internal;
using Rin.Platform.Vulkan;
using Serilog;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using Key = Silk.NET.Input.Key;
using MouseButton = Silk.NET.Input.MouseButton;
using WindowOptions = Rin.Core.Abstractions.WindowOptions;

[assembly: InternalsVisibleTo("Rin.Core")]
[assembly: InternalsVisibleTo("Rin.Editor")]

namespace Rin.Platform.Silk;

sealed class SilkWindow : Abstractions.Rendering.IWindow {
    readonly ILogger log = Log.ForContext<Abstractions.Rendering.IWindow>();
    internal static SilkWindow MainWindow;
    internal global::Silk.NET.Windowing.IWindow silkWindow = null!;
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

    public Size Size => new(silkWindow.Size.X, silkWindow.Size.Y);
    public RendererContext RendererContext { get; private set; }
    public ISwapchain Swapchain { get; private set; }
    public Vector2 MousePosition { get; private set; } = Vector2.Zero;

    internal SilkWindow(WindowOptions options) {
        MainWindow = this;
        Initialize(options);
    }

    // public IInternalGuiRenderer CreateGuiRenderer() => throw new NotImplementedException();
    // new SilkImGuiRenderer(new(Gl, silkWindow, input));

    public bool GetKey(Core.Abstractions.Key key) => keyPressed[(int)key];
    public bool GetKeyDown(Core.Abstractions.Key key) => keyDown.HasValue && (int)keyDown.Value == (int)key;
    public bool GetKeyUp(Core.Abstractions.Key key) => keyUp.HasValue && (int)keyUp.Value == (int)key;

    public Vector2 GetMouseAxis() => mouseAxis;

    public bool GetMouseButtonDown(Core.Abstractions.MouseButton mouseButton) =>
        mouseButtonDown.HasValue && (int)mouseButtonDown.Value == (int)mouseButton;

    public bool GetMouseButtonUp(Core.Abstractions.MouseButton mouseButton) =>
        mouseButtonUp.HasValue && (int)mouseButtonUp.Value == (int)mouseButton;

    void Initialize(WindowOptions options) {
        silkWindow = Window.Create(
            global::Silk.NET.Windowing.WindowOptions.DefaultVulkan with {
                Title = options.Title, Size = new(options.Size.Width, options.Size.Height), VSync = options.VSync
            }
        );

        silkWindow.Load += OnLoad;
        silkWindow.Closing += OnClosing;
        silkWindow.Resize += OnResize;

        silkWindow.Initialize();
        RendererContext = ObjectFactory.CreateRendererContext();
        var swapChain = new VulkanSwapChain();
        swapChain.InitializeSurface(silkWindow);

        var size = new Size(silkWindow.Size.X, silkWindow.Size.Y);
        swapChain.Create(ref size, false);
        Swapchain = swapChain;
    }

    void OnResize(Vector2D<int> obj) {
        Resize?.Invoke(new(obj.X, obj.Y));
    }

    void OnClosing() {
        Closing?.Invoke();
    }

    void ResetInput() {
        keyDown = null;
        keyUp = null;
        mouseAxis = Vector2.Zero;
        mouseButtonDown = null;
        mouseButtonUp = null;
    }

    void OnLoad() {
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

        log.Debug("Silk Window Loaded");
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

    public event Action? Closing;
    public event Action<Size>? Resize;
}
