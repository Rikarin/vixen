using ImGuiNET;

namespace Rin.Core.UI;

public class DragInt : Drag {
    readonly State<int> value;
    readonly Range range;
    readonly float speed;
    readonly string format;

    public DragInt(State<int> value, Range range, float speed, string format = "%d") {
        this.value = value;
        this.range = range;
        this.speed = speed;
        this.format = format;
    }

    public override void Render() {
        var value = this.value.Value;
        if (ImGui.DragInt($"###{ViewContext.GetId()}", ref value, speed, range.Start.Value, range.End.Value, format)) {
            this.value.SetNext(value);
        }
        
        base.Render();
    }
}
