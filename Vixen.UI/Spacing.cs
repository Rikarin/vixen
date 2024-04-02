using ImGuiNET;

namespace Vixen.UI;

public class Spacing : View {
    public override void Render() {
        ImGui.Spacing();
        base.Render();
    }
}
