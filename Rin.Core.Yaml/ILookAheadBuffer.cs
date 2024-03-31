namespace Rin.Core.Yaml;

interface ILookAheadBuffer {
    /// <summary>
    ///     Gets a value indicating whether the end of the input reader has been reached.
    /// </summary>
    bool EndOfInput { get; }

    /// <summary>
    ///     Gets the character at the specified offset.
    /// </summary>
    char Peek(int offset);

    /// <summary>
    ///     Skips the next <paramref name="length" /> characters. Those characters must have been
    ///     obtained first by calling the <see cref="Peek" /> method.
    /// </summary>
    void Skip(int length);
}
