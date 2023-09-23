using ImGuiNET;

namespace Rin.Editor.Panes;

sealed class ConsolePane : Pane {
    public ConsolePane() : base("Console") { }

    protected override void OnRender() {
        ImGui.Text("TODO");
    }
}
