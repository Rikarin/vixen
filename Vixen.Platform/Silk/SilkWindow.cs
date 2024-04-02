using Serilog;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using System.Drawing;
using System.Runtime.CompilerServices;
using Vixen.Core.Common;
using Vixen.InputSystem;
using Vixen.Platform.Common.Rendering;
using Vixen.Platform.Internal;
using Vixen.Platform.Silk.Input;
using Vixen.Platform.Vulkan;
using IWindow = Vixen.Platform.Common.Rendering.IWindow;
using Rendering_IWindow = Vixen.Platform.Common.Rendering.IWindow;
using WindowOptions = Vixen.Core.Common.WindowOptions;

[assembly: InternalsVisibleTo("Vixen.Core")]
[assembly: InternalsVisibleTo("Vixen.Editor")]

namespace Vixen.Platform.Silk;

sealed class SilkWindow : Rendering_IWindow {
    readonly InputManager inputManager;
    public ImGuiController imGuiController;
    internal static SilkWindow MainWindow;
    internal global::Silk.NET.Windowing.IWindow silkWindow = null!;
    readonly ILogger log = Log.ForContext<IWindow>();

    SilkInputSource inputSource;

    public Size Size => new(silkWindow.Size.X, silkWindow.Size.Y);
    public RendererContext RendererContext { get; private set; }
    public ISwapchain Swapchain { get; private set; }

    internal SilkWindow(WindowOptions options, InputManager inputManager) {
        this.inputManager = inputManager;
        MainWindow = this;
        Initialize(options);
    }

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
        swapChain.Create(ref size, options.VSync);
        Swapchain = swapChain;

        // TODO: move this away
        imGuiController = new(
            VulkanContext.Vulkan,
            silkWindow,
            inputSource.SilkInputContext,
            new("Assets/Switzer/Switzer-Semibold.ttf", 16),
            VulkanContext.CurrentDevice.PhysicalDevice.VkPhysicalDevice,
            VulkanContext.CurrentDevice.PhysicalDevice.QueueFamilyIndices.Graphics.Value,
            swapChain.Images.Count(),
            swapChain.ColorFormat,
            null
        );
    }

    void OnResize(Vector2D<int> obj) => Resize?.Invoke(new(obj.X, obj.Y));
    void OnClosing() => Closing?.Invoke();

    void OnLoad() {
        inputSource = new(silkWindow);
        inputManager.Sources.Add(inputSource);
        
        log.Debug("Silk Window Loaded");
    }

    public event Action? Closing;
    public event Action<Size>? Resize;
}
