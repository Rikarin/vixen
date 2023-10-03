using Rin.Core.Diagnostics;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Rin.Editor")]

namespace Rin.Core.General;

public class Application {
    internal static Application Current = null!;

    readonly Performance performance = new();

    public Window MainWindow { get; }

    public Application(ApplicationOptions options) {
        ApplicationEventSource.Log.Startup();
        Current = this;


        // Setup profiler
        // Setup Renderer.SetConfig (static)

        MainWindow = new();

        // Renderer.Initialize();
    }

    // ConcurrentQueue<Action> queue = new();
    // bool running;

    public void InvokeOnMainThread(Action action) {
        // queue.Enqueue(action);
    }

    public void Run() {
        performance.MainThreadWaitTime.Reset();

        // block till render is complete

        ApplicationEventSource.Log.ReportMainThreadWaitTime(performance.MainThreadWaitTime.ElapsedMilliseconds);


        MainWindow.Run();
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

    struct Performance {
        public Stopwatch MainThreadWorkTime { get; } = new();
        public Stopwatch MainThreadWaitTime { get; } = new();

        public Performance() { }
    }
}
