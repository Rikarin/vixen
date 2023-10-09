using ImGuiNET;

namespace Rin.Core.UI;

public class Picker : View {
    readonly State<string> selection;
    readonly List<string> options;

    readonly string? label;

    public Picker(string? label, State<string> selection, IEnumerable<string> options) {
        this.label = label;
        this.selection = selection;
        this.options = options.ToList();
    }

    // public Picker OnSelect(Action<string> action) {
    //     onSelect = action;
    //     return this;
    // }

    public override void Render() {
        if (ImGui.BeginCombo($"{label}###{ViewContext.GetId()}", selection.Value)) {
            for (var i = 0; i < options.Count; i++) {
                if (ImGui.Selectable(options[i], selection.Value == options[i])) {
                    selection.SetNext(options[i]);
                }
            }

            ImGui.EndPopup();
        }
        
        base.Render();
    }
}