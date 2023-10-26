namespace Rin.Editor;

public class RangeAttribute : Attribute {
    /// <summary>
    ///     Inclusive
    /// </summary>
    public float From { get; }

    /// <summary>
    ///     Exclusive
    /// </summary>
    public float To { get; }

    public RangeAttribute(float from, float to) {
        From = from;
        To = to;
    }
}
