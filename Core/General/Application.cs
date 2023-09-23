using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Rin.Editor")]

namespace Rin.Core.General;

public class Application {
    internal static Application Current = null!;

    public Window MainWindow { get; }

    public Application() {
        Current = this;
        MainWindow = new();
        MainWindow.Load += () => Load?.Invoke();
        MainWindow.Closing += () => Closing?.Invoke();
        MainWindow.Render += deltaTime => Render?.Invoke(deltaTime);
    }

    // ConcurrentQueue<Action> queue = new();
    // bool running;

    public void InvokeOnMainThread(Action action) {
        // queue.Enqueue(action);
    }

    public void Run() {
        MainWindow.Run();
    }

    public event Action? Load;
    public event Action? Closing;
    public event Action<float>? Render;

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
