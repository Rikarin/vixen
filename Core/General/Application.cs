using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Rin.Editor")]

namespace Rin.Core.General;

public class Application {
    // TODO: this shouldn't be static
    internal static Window Window;

    public event Action? Load;
    public event Action<float>? Render;

    public Application() {
        Window = new();
        Window.Load += () => Load?.Invoke();
        Window.Render += deltaTime => Render?.Invoke(deltaTime);
    }

    // ConcurrentQueue<Action> queue = new();
    // bool running;

    public void InvokeOnMainThread(Action action) {
        // queue.Enqueue(action);
    }

    public void Run() {
        Window.Run();
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
