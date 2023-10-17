using ImGuiNET;

namespace Rin.UI;

public class Text : View {
    readonly string text;

    public Text(string text) {
        this.text = text;
    }

    public override void Render() {
        ImGui.Text(text);
        base.Render();
    }
}
