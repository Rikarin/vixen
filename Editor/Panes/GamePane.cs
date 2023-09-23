using ImGuiNET;

namespace Rin.Editor.Panes;

sealed class GamePane : Pane {
    public GamePane() : base("Game") { }

    protected override void OnRender() {
        ImGui.Text("TODO");
    }
}
