using ImGuiNET;

namespace Rin.Editor.Panes;

sealed class ConsolePane : Pane {
    public ConsolePane() : base("Console") { }

    int value = 0;

    protected override void OnRender() {
        ImGui.Text("TODO");
        ImGui.Button("Button text");
        ImGui.SliderInt("test", ref value, 0, 200);
    }
}
