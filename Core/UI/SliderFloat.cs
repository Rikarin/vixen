using ImGuiNET;

namespace Rin.Core.UI;

public class SliderFloat : Slider {
    readonly State<float> value;
    readonly Range range;
    readonly string format;

    public SliderFloat(State<float> value, Range range, string format = "%.3f") {
        this.value = value;
        this.range = range;
        this.format = format;
    }

    public override void Render() {
        var value = this.value.Value;
        if (ImGui.SliderFloat($"###{ViewContext.GetId()}", ref value, range.Start.Value, range.End.Value, format)) {
            this.value.SetNext(value);
        }

        base.Render();
    }
}