using Rin.Core.Abstractions;
using Rin.Core.Diagnostics;
using System.Diagnostics;
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

        MainWindow = new();
        Renderer.Initialize(MainWindow.Handle.Swapchain);
        renderThread.Pump();

        // Setup profiler
        // Setup Renderer.SetConfig (static)


        // Renderer.Initialize();
    }

    // ConcurrentQueue<Action> queue = new();
    // bool running;

    public void InvokeOnMainThread(Action action) {
        // queue.Enqueue(action);
    }

    public void Run() {
        MainWindow.Test_InvokeLoad();
        IsRunning = true;

        while (IsRunning) {
            // TODO: consider creating disposable struct/class to track these states
            performance.MainThreadWaitTime.Reset();
            renderThread.BlockUntilRenderComplete();
            ApplicationEventSource.Log.ReportMainThreadWaitTime(performance.MainThreadWaitTime.ElapsedMilliseconds);

            renderThread.NextFrame();
            renderThread.Kick();

            // TODO: if not minimized

            Renderer.Submit(MainWindow.Handle.Swapchain.BeginFrame);

            Renderer.Submit(MainWindow.Handle.Swapchain.Present);

            Renderer.IncreaseCurrentFrameIndex();
        }

        // MainWindow.Run();
    }

    // public void Run() {
    //     while (running) {
    //         ExecuteMainThreadQueue();
    //     }
    // }

    // void ExecuteMainThreadQueue() {
    //     while (queue.TryDequeue(out var action)) {
    //         action();
    //     }
    // }

    public static Application CreateDefault(Action<ApplicationOptions>? configureOptions = null) {
        var options = new ApplicationOptions { Name = "Rin Engine", ThreadingPolicy = ThreadingPolicy.MultiThreaded };
        configureOptions?.Invoke(options);

        return new(options);
    }

    public void Dispose() {
        renderThread.Terminate();
    }

    struct Performance {
        public Stopwatch MainThreadWorkTime { get; } = new();
        public Stopwatch MainThreadWaitTime { get; } = new();

        public Performance() { }
    }
}
