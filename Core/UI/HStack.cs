using ImGuiNET;

namespace Rin.Core.UI;

public class HStack : View {
    readonly View[] items;

    public HStack(View[] items) {
        this.items = items;
    }

    public override void Render() {
        items[0].Render();
        
        foreach (var item in items[1..]) {
            ImGui.SameLine();
            item.Render();
        }
        
        base.Render();
    }
}
