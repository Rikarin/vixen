using ImGuiNET;

namespace Vixen.UI;

public class TextField : View {
    readonly State<string> text;
    readonly string? label;

    public TextField(string? label, State<string> text) {
        this.label = label;
        this.text = text;
    }

    public override void Render() {
        var value = text.Value ?? "";

        if (ImGui.InputText($"{label}###{ViewContext.GetId()}", ref value, 256)) {
            text.SetNext(value);
        }
        
        base.Render();
    }
}