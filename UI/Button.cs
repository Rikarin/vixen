using ImGuiNET;
using Rin.Core.General;
using System.Drawing;

namespace Rin.UI;

public class Button : View {
    readonly string label;
    Action? onSubmit;

    // public Button(string label, Image image, Action? onSubmit) {
    // public Button(Action? onSubmit, View label) {
    
    public Button(string label, Action? onSubmit) {
        this.label = label;
        this.onSubmit = onSubmit;
    }
    
    public Button Background(Color color) => this;

    public Button Font(Font font) => this;

    public Button OnSubmit(Action action) {
        onSubmit = action;
        return this;
    }

    public override void Render() {
        if (ImGui.Button($"{label}###{ViewContext.GetId()}")) {
            onSubmit?.Invoke();
        }
        
        // This will render context menu
        base.Render();
    }
}