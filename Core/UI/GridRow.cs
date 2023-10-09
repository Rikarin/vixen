using ImGuiNET;

namespace Rin.Core.UI;

public class GridRow : View {
    readonly View[] children;

    public int Count => children.Length;

    public GridRow(View[] children) {
        this.children = children;
    }

    public override void Render() {
        foreach (var child in children) {
            child.Render();
            ImGui.NextColumn();
        }

        base.Render();
    }
}