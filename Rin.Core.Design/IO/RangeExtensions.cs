namespace Rin.Core.Design.IO;

static class RangeExtensions {
    public static bool IsValid(this Range range) {
        return range.Start.Value > 0 && range.End.Value > 0;
    }
}
