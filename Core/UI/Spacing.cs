using ImGuiNET;

namespace Rin.Core.UI;

public class Spacing : View {
    public override void Render() {
        ImGui.Spacing();
        base.Render();
    }
}
