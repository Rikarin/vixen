using ImGuiNET;

namespace Rin.UI;

public class Menu : View {
    readonly string label;
    readonly View[] items;

    public Menu(string label, View[] items) {
        this.label = label;
        this.items = items;
    }

    public override void Render() {
        if (ImGui.BeginMenu(label)) {
            foreach (var item in items) {
                item.Render();
            }

            ImGui.EndMenu();
        }
        
        base.Render();
    }
}
