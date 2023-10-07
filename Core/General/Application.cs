using Rin.Core.Abstractions;
using Rin.Core.Diagnostics;
using Rin.Platform.Silk;
using Rin.Platform.Vulkan;
using Rin.Rendering;
using Serilog;
using System.Drawing;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Rin.Editor")]

namespace Rin.Core.General;

public class Application : IDisposable {
    internal static Application Current = null!;

    readonly ILogger log = Log.ForContext<Application>();
    readonly IRenderThread renderThread;

    public bool IsRunning { get; private set; }
    public bool IsMinimized {get; private set; }
    public Window MainWindow { get; }

    public Application(ApplicationOptions options) {
        ApplicationEventSource.Log.Startup();
        Current = this;

        Renderer.Options.FramesInFlight = 3; // TODO

        renderThread = new RenderThread(options.ThreadingPolicy);
        renderThread.Run();

        // TODO: stuff

        MainWindow = new(
            o => {
                o.Title = options.Name;
                o.Size = options.WindowSize;
            });
        
        MainWindow.Resize += OnWindowResize;
        MainWindow.Closing += OnWindowClose;
        
        var vulkanRenderer = new VulkanRenderer();
        Renderer.Initialize(MainWindow.Handle.Swapchain, vulkanRenderer);
        renderThread.Pump();

        // Setup profiler
        // Setup Renderer.SetConfig (static)
    }

    // ConcurrentQueue<Action> queue = new();
    // bool running;

    public void InvokeOnMainThread(Action action) {
        // queue.Enqueue(action);
    }

    public void Run() {
        IsRunning = true;
        var silkWindow = MainWindow.Handle as SilkWindow;

        while (IsRunning) {
            using (var _ = ApplicationEventSource.MainThreadWaitTime) {
                renderThread.BlockUntilRenderComplete();
            }
            
            log.Verbose("============= APPLICATION ======================");
            silkWindow.silkWindow.DoEvents();

            renderThread.NextFrame();
            renderThread.Kick();

            // TODO: if not minimized
            if (!IsMinimized) {
                using var workTimeWatcher = ApplicationEventSource.MainThreadWorkTime;

                Renderer.Submit(MainWindow.Handle.Swapchain.BeginFrame);
                Renderer.BeginFrame();

                Update?.Invoke();

                Renderer.EndFrame();
                Renderer.Submit(MainWindow.Handle.Swapchain.Present);

                Renderer.IncreaseCurrentFrameIndex();

                // silkWindow.silkWindow.DoUpdate();
                // silkWindow.silkWindow.DoRender();
            }
        }

        Log.Information("stopped working");
        
        renderThread.Terminate();

    //     while (running) {
    //         ExecuteMainThreadQueue();
    //     }
    }

    // void ExecuteMainThreadQueue() {
    //     while (queue.TryDequeue(out var action)) {
    //         action();
    //     }
    // }

    void OnWindowResize(Size newSize) {
        Log.Information("OnResize: {Variable}", newSize);
        Renderer.Submit(() => MainWindow.Handle.Swapchain.OnResize(newSize));
    }

    void OnWindowClose() {
        Log.Information("on close");
        IsRunning = false;
    }

    public static Application CreateDefault(Action<ApplicationOptions>? configureOptions = null) {
        var options = new ApplicationOptions { Name = "Rin Engine", ThreadingPolicy = ThreadingPolicy.MultiThreaded };
        configureOptions?.Invoke(options);

        return new(options);
    }

    public void Dispose() {
        renderThread.Terminate();

        MainWindow.Resize -= OnWindowResize;
        MainWindow.Closing -= OnWindowClose;
    }

    public event Action? Update;
}
