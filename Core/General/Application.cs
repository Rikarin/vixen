using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Rin.Editor")]

namespace Rin.Core.General;

public class Application {
    internal static Application Current = null!;

    public Window MainWindow { get; }

    public Application() {
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
}
