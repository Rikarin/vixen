using ImGuiNET;

namespace Rin.UI;

public class Spacing : View {
    public override void Render() {
        ImGui.Spacing();
        base.Render();
    }
}
