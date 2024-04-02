namespace Vixen.Core.Yaml;

/// <summary>
///     Provides access to a stream and allows to peek at the next characters,
///     up to the buffer's capacity.
/// </summary>
/// <remarks>
///     This class implements a circular buffer with a fixed capacity.
/// </remarks>
public class LookAheadBuffer : ILookAheadBuffer {
    readonly TextReader input;
    readonly char[] buffer;
    int firstIndex;
    int count;
    bool endOfInput;

    /// <summary>
    ///     Gets a value indicating whether the end of the input reader has been reached.
    /// </summary>
    public bool EndOfInput => endOfInput && count == 0;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LookAheadBuffer" /> class.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="capacity">The capacity.</param>
    public LookAheadBuffer(TextReader? input, int capacity) {
        if (capacity < 1) {
            throw new ArgumentOutOfRangeException(nameof(capacity), "The capacity must be positive.");
        }

        this.input = input ?? throw new ArgumentNullException(nameof(input));
        buffer = new char[capacity];
    }

    /// <summary>
    ///     Gets the character at thhe specified offset.
    /// </summary>
    public char Peek(int offset) {
        if (offset < 0 || offset >= buffer.Length) {
            throw new ArgumentOutOfRangeException(
                nameof(offset),
                "The offset must be between zero and the capacity of the buffer."
            );
        }

        Cache(offset);

        if (offset < count) {
            return buffer[GetIndexForOffset(offset)];
        }

        return '\0';
    }

    /// <summary>
    ///     Reads characters until at least <paramref name="length" /> characters are in the buffer.
    /// </summary>
    /// <param name="length">
    ///     Number of characters to cache.
    /// </param>
    public void Cache(int length) {
        while (length >= count) {
            var nextChar = input.Read();
            if (nextChar >= 0) {
                var lastIndex = GetIndexForOffset(count);
                buffer[lastIndex] = (char)nextChar;
                ++count;
            } else {
                endOfInput = true;
                return;
            }
        }
    }

    /// <summary>
    ///     Skips the next <paramref name="length" /> characters. Those characters must have been
    ///     obtained first by calling the <see cref="Peek" /> or <see cref="Cache" /> methods.
    /// </summary>
    public void Skip(int length) {
        if (length < 1 || length > count) {
            throw new ArgumentOutOfRangeException(
                nameof(length),
                "The length must be between 1 and the number of characters in the buffer. Use the Peek() and / or Cache() methods to fill the buffer."
            );
        }

        firstIndex = GetIndexForOffset(length);
        count -= length;
    }

    /// <summary>
    ///     Gets the index of the character for the specified offset.
    /// </summary>
    int GetIndexForOffset(int offset) {
        var index = firstIndex + offset;
        if (index >= buffer.Length) {
            index -= buffer.Length;
        }

        return index;
    }
}
