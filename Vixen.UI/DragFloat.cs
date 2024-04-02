using ImGuiNET;

namespace Vixen.UI;

public class DragFloat : Drag {
    readonly State<float> value;
    readonly Range range;
    readonly float speed;
    readonly string format;

    public DragFloat(State<float> value, Range range, float speed, string format = "%.3f") {
        this.value = value;
        this.range = range;
        this.speed = speed;
        this.format = format;
    }

    public override void Render() {
        var value = this.value.Value;
        if (ImGui.DragFloat($"###{ViewContext.GetId()}", ref value, speed, range.Start.Value, range.End.Value, format)) {
            this.value.SetNext(value);
        }
        
        base.Render();
    }
}