using ImGuiNET;

namespace Rin.UI;

public class GridRow : View {
    readonly View[] children;

    public int Count => children.Length;

    public GridRow(View[] children) {
        this.children = children;
    }

    public override void Render() {
        children[0].Render();
        
        foreach (var child in children[1..]) {
            ImGui.NextColumn();
            child.Render();
        }

        base.Render();
    }
}