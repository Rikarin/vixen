using ImGuiNET;

namespace Rin.Core.UI;

public class CollapsingView : View {
    readonly string label;
    readonly View child;

    public CollapsingView(string label, View child) {
        this.label = label;
        this.child = child;
    }

    public override void Render() {
        if (ImGui.CollapsingHeader(label, ImGuiTreeNodeFlags.DefaultOpen)) {
            child.Render();
        }
        
        base.Render();
    }
}
