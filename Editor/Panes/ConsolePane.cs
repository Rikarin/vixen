using ImGuiNET;

namespace Rin.Editor.Panes;

sealed class ConsolePane : Pane {
    public ConsolePane() : base("Console") { }

    protected override void OnRender() {
        if (ImGui.Button("Clear")) {
            EditorSink.Messages.Clear();
        }

        if (ImGui.BeginChild("console")) {
            foreach (var message in EditorSink.Messages) {
                ImGui.Text(message);
            }

            ImGui.EndChild();
        }
    }
}
