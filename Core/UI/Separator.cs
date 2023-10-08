using ImGuiNET;

namespace Rin.Core.UI;

public class Separator : View {
    public override void Render() {
        ImGui.Separator();

        // This will render context menu
        // base.Render();
    }
}
