using Rin.UI;

namespace Rin.Editor.Panes;

sealed class ConsolePane : Pane {
    public ConsolePane() : base("Console") { }

    protected override void OnRender() {
        // if (ImGui.BeginChild("console")) {
            new ConsoleView().Render();
            // ImGui.EndChild();
        // }
    }
}


public class ConsoleView : View {
    // @formatter:off
    protected override View Body =>
        VStack(
            Button("Clear", () => EditorSink.Messages.Clear()),
            ForEach(EditorSink.Messages, Text)
        );
    // formatter:on
}