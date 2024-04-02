namespace Vixen.Core.Common;

public sealed class RenderCommandQueue {
    readonly Queue<Action> commands = new();

    public int Count => commands.Count;

    public void Push(Action command) => commands.Enqueue(command);

    public void Execute() {
        while (commands.TryDequeue(out var cmd)) {
            cmd();
        }
    }
}
