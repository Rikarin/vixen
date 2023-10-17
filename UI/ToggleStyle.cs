namespace Rin.UI;

public class ToggleStyle : PushConfiguration {
    public static ToggleStyle Button = new();
    public static ToggleStyle Switch = new();

    public override void OnSet() {
        // ImGui.PushStyleColor();
    }

    public override void OnReset() {
        // ImGui.PopStyleColor();
    }
}