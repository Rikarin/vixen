using System.Drawing;
using System.Numerics;

namespace Rin.Editor.Panes;

// ImGui.PushStyleColor(ImGuiCol.FrameBg, Color.Red.ToVector4());
// ImGui.PopStyleColor();
static class ColorExtensions {
    public static Vector4 ToVector4(this Color color) =>
        new(color.R / 256f, color.G / 256f, color.B / 256f, color.A / 256f);
}
