using ImGuiNET;

namespace Rin.Core.UI;

public class SliderInt : Slider {
    readonly State<int> value;
    readonly Range range;
    readonly string format;

    public SliderInt(State<int> value, Range range, string format = "%d") {
        this.value = value;
        this.range = range;
        this.format = format;
    }

    public override void Render() {
        var value = this.value.Value;
        if (ImGui.SliderInt($"###{ViewContext.GetId()}", ref value, range.Start.Value, range.End.Value, format)) {
            this.value.SetNext(value);
        }

        base.Render();
    }
}
