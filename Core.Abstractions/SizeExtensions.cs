using System.Drawing;

namespace Rin.Core.Abstractions;

public static class SizeExtensions {
    public static Size Multiply(this in Size size, float scale) =>
        new((int)(size.Width * scale), (int)(size.Height * scale));
}
