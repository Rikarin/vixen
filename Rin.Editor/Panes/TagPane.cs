using ImGuiNET;

namespace Rin.Editor.Panes;

sealed class TagPane : Pane {
    public TagPane() : base("Tags") { }

    protected override void OnRender() {
        ImGui.Text("TODO");
    }
}
