using ImGuiNET;

namespace Rin.UI;

public class Grid : View {
    readonly View[] children;

    public Grid(View[] children) {
        this.children = children;
    }

    public override void Render() {
        var columns = GetMaxColumns();
        
        // TODO: fix this somehow
        // ImGui.BeginGroup();
        // if (ImGui.BeginChild($"###{ViewContext.GetId()}", new (-1, -1), false, ImGuiWindowFlags.None)) {
            // ImGui.Columns(columns, "foo", true);
            // ImGui.Columns(child is GridRow ? columns : 1, null, false);
            foreach (var child in children) {
                ImGui.Columns(child is GridRow ? columns : 1, null, false);
                child.Render();
                ImGui.Columns(1);
            }

            // ImGui.EndChild();
        // }
        // ImGui.EndGroup();

        base.Render();
    }

    int GetMaxColumns() => children.Select(x => x is GridRow g ? g.Count : 1).Max();
}
