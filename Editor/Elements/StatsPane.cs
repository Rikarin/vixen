using ImGuiNET;

namespace Rin.Editor.Elements;

class StatsPane : Pane {
    public StatsPane() : base("Stats") {
        
    }
    
    protected override void OnRender() {
        ImGui.Text("Renderer Stats:");
        ImGui.Text("Draw Calls: 42");
        ImGui.Text("Quads: 42");
        ImGui.Text("Vertices: 42");
        ImGui.Text("Indices: 42");
    }
}
