using ImGuiNET;

namespace Rin.Editor.Panes;

sealed class ScenePane : Pane {
    public ScenePane() : base("Scene") { }

    protected override void OnRender() {
        ImGui.Text("TODO");
    }
}
