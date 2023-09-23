using ImGuiNET;

namespace Rin.Editor.Elements; 

class TagPane : Pane {
    public TagPane() : base("Tags") {
        
    }

    protected override void OnRender() {
        ImGui.Text("TODO");
    }
}
