using ImGuiNET;

namespace Rin.Core.UI;

public class Toggle : View {
    readonly State<bool> isOn;
    string label;

    public Toggle(string label, State<bool> isOn) {
        this.label = label;
        this.isOn = isOn;
    }

    // public Toggle Label(string label) {
    //     this.label = label;
    //     return this;
    // }

    public override void Render() {
        var value = isOn.Value;

        if (ImGui.Checkbox(label, ref value)) {
            isOn.SetNext(value);
        }
    }
}