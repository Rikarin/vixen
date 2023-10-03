namespace Rin.Core.Abstractions;

public sealed class RenderCommandQueue {
    readonly Queue<Action> commands = new();

    public void Push(Action command) => commands.Enqueue(command);

    public void Execute() {
        while (commands.TryDequeue(out var cmd)) {
            cmd();
        }
    }
}
