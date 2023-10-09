using ImGuiNET;
using System.Numerics;

namespace Rin.Core.UI;

public class Grid : View {
    readonly View[] children;

    public Grid(View[] children) {
        this.children = children;
    }

    public override void Render() {
        var columns = GetMaxColumns();
        
        if (ImGui.BeginChild($"###{ViewContext.GetId()}", Vector2.Zero, false, ImGuiWindowFlags.NoBackground)) {
            foreach (var child in children) {
                ImGui.Columns(child is GridRow ? columns : 1, null, false);
                child.Render();
                ImGui.Columns(1);
            }

            ImGui.EndChild();
        }

        base.Render();
    }

    int GetMaxColumns() => children.Select(x => x is GridRow g ? g.Count : 1).Max();
}
