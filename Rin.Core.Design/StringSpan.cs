using System.Runtime.InteropServices;

namespace Rin.Core.Design;

/// <summary>
///     A region of character in a string.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public record struct StringSpan {
    /// <summary>
    ///     The start offset of the span.
    /// </summary>
    public int Start;

    /// <summary>
    ///     The length of the span
    /// </summary>
    public int Length;

    /// <summary>
    ///     Gets a value indicating whether this instance is valid (Start greater or equal to 0, and Length greater than 0)
    /// </summary>
    /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
    public bool IsValid => Start >= 0 && Length > 0;

    /// <summary>
    ///     Gets the next position = Start + Length.
    /// </summary>
    /// <value>The next.</value>
    public int Next => Start + Length;

    /// <summary>
    ///     The end offset of the span.
    /// </summary>
    public int End => Start + Length - 1;

    /// <summary>
    ///     Initializes a new instance of the <see cref="StringSpan" /> struct.
    /// </summary>
    /// <param name="start">The start.</param>
    /// <param name="length">The length.</param>
    public StringSpan(int start, int length) {
        Start = start;
        Length = length;
    }

    public override string ToString() => IsValid ? $"[{Start}-{End}]" : "[N/A]";
}

public static class StringExtensions {
    /// <summary>
    ///     Gets the substring with the specified span. If the span is invalid, return null.
    /// </summary>
    /// <param name="str">The string.</param>
    /// <param name="span">The span.</param>
    /// <returns>A substring with the specified span or null if span is empty.</returns>
    public static string? Substring(this string str, StringSpan span) =>
        span.IsValid ? str.Substring(span.Start, span.Length) : null;
}
