using ImGuiNET;
using Rin.Core.General;
using System.Drawing;

namespace Rin.Core.UI;

public class MenuItem : View {
    readonly string label;
    Action? onSubmit;

    public MenuItem(string label, Action? onSubmit) {
        this.label = label;
        this.onSubmit = onSubmit;
    }
    
    // public Button Background(Color color) => this;
    //
    // public Button Font(Font font) => this;

    public MenuItem OnSubmit(Action action) {
        onSubmit = action;
        return this;
    }

    public override void Render() {
        if (ImGui.MenuItem($"{label}###{ViewContext.GetId()}")) {
            onSubmit?.Invoke();
        }
        
        base.Render();
    }
}
