using ImGuiNET;
using Serilog;

namespace Rin.Core.UI;

public class ContextMenuView : View {
    readonly View[] contextMenu;

    public ContextMenuView(View[] contextMenu) {
        this.contextMenu = contextMenu;
    }

    public override void Render() {
        if (ImGui.BeginPopupContextItem()) {
            foreach (var item in contextMenu) {
                item.Render();
            }

            ImGui.EndPopup();
        }
    }
}