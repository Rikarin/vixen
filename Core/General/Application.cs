using Rin.Core.Abstractions;
using Rin.Core.Diagnostics;
using Rin.Platform.Silk;
using Rin.Platform.Vulkan;
using Rin.Rendering;
using Serilog;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Rin.Editor")]

namespace Rin.Core.General;

public class Application : IDisposable {
    internal static Application Current = null!;

    readonly Performance performance = new();
    readonly IRenderThread renderThread;

    public bool IsRunning { get; private set; }
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
        // MainWindow.Test_InvokeLoad();
        IsRunning = true;
        
        var silkWindow = MainWindow.Handle as SilkWindow;
        // silkWindow.silkWindow.Initialize();

        while (IsRunning) {
            // TODO: consider creating disposable struct/class to track these states
            performance.MainThreadWaitTime.Reset();
            renderThread.BlockUntilRenderComplete();
            ApplicationEventSource.Log.ReportMainThreadWaitTime(performance.MainThreadWaitTime.ElapsedMilliseconds);
            
            Log.Information("============= INVOKING ======================");
            silkWindow.silkWindow.DoEvents();

            renderThread.NextFrame();
            renderThread.Kick();

            // TODO: if not minimized

            Renderer.Submit(MainWindow.Handle.Swapchain.BeginFrame);
            Renderer.BeginFrame();
            
            Render?.Invoke();

            Renderer.EndFrame();
            Renderer.Submit(MainWindow.Handle.Swapchain.Present);

            Renderer.IncreaseCurrentFrameIndex();

            // silkWindow.silkWindow.DoUpdate();
            // silkWindow.silkWindow.DoRender();
        }

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

    public static Application CreateDefault(Action<ApplicationOptions>? configureOptions = null) {
        var options = new ApplicationOptions { Name = "Rin Engine", ThreadingPolicy = ThreadingPolicy.MultiThreaded };
        configureOptions?.Invoke(options);

        return new(options);
    }

    public void Dispose() {
        renderThread.Terminate();

        MainWindow.Resize -= OnWindowResize;
    }

    public event Action? Render;

    struct Performance {
        public Stopwatch MainThreadWorkTime { get; } = new();
        public Stopwatch MainThreadWaitTime { get; } = new();

        public Performance() { }
    }
}
