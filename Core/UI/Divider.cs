using ImGuiNET;

namespace Rin.Core.UI;

public class Divider : View {
    public override void Render() {
        ImGui.Separator();
        base.Render();
    }
}
