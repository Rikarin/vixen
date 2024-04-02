using ImGuiNET;

namespace Vixen.Editor.Panes;

sealed class TagPane : Pane {
    public TagPane() : base("Tags") { }

    protected override void OnRender() {
        ImGui.Text("TODO");
    }
}
