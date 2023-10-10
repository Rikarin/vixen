using ImGuiNET;

namespace Rin.Core.UI;

public class HStack : View {
    readonly View[] children;

    public HStack(View[] children) {
        this.children = children;
    }

    public override void Render() {
        children[0].Render();
        
        foreach (var child in children[1..]) {
            ImGui.SameLine();
            child.Render();
        }
        
        base.Render();
    }
}
