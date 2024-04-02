using System.Diagnostics;

namespace Vixen.Core.Yaml;

class CharacterAnalyzer<TBuffer>(TBuffer buffer) where TBuffer : ILookAheadBuffer {
    public TBuffer Buffer { get; } = buffer;
    public bool EndOfInput => Buffer.EndOfInput;
    public char Peek(int offset) => Buffer.Peek(offset);

    public void Skip(int length) {
        Buffer.Skip(length);
    }

    /// <summary>
    ///     Check if the character at the specified position is an alphabetical
    ///     character, a digit, '_', or '-'.
    /// </summary>
    public bool IsAlpha(int offset) {
        var character = Buffer.Peek(offset);
        return character is >= '0' and <= '9' or >= 'A' and <= 'Z' or >= 'a' and <= 'z' or '_' or '-';
    }

    public bool IsAlpha() => IsAlpha(0);

    /// <summary>
    ///     Check if the character is ASCII.
    /// </summary>
    public bool IsAscii(int offset) => Buffer.Peek(offset) <= '\x7F';

    public bool IsAscii() => IsAscii(0);

    public bool IsPrintable(int offset) {
        var character = Buffer.Peek(offset);
        return Emitter.IsPrintable(character);
    }

    public bool IsPrintable() => IsPrintable(0);

    /// <summary>
    ///     Check if the character at the specified position is a digit.
    /// </summary>
    public bool IsDigit(int offset) {
        var character = Buffer.Peek(offset);
        return character is >= '0' and <= '9';
    }

    public bool IsDigit() => IsDigit(0);

    /// <summary>
    ///     Get the value of a digit.
    /// </summary>
    public int AsDigit(int offset) => Buffer.Peek(offset) - '0';

    public int AsDigit() => AsDigit(0);

    /// <summary>
    ///     Check if the character at the specified position is a hex-digit.
    /// </summary>
    public bool IsHex(int offset) {
        var character = Buffer.Peek(offset);
        return character is >= '0' and <= '9' or >= 'A' and <= 'F' or >= 'a' and <= 'f';
    }

    /// <summary>
    ///     Get the value of a hex-digit.
    /// </summary>
    public int AsHex(int offset) {
        var character = Buffer.Peek(offset);

        if (character <= '9') {
            return character - '0';
        }

        if (character <= 'F') {
            return character - 'A' + 10;
        }

        return character - 'a' + 10;
    }

    public bool IsSpace(int offset) => Check(' ', offset);

    public bool IsSpace() => IsSpace(0);

    /// <summary>
    ///     Check if the character at the specified position is NUL.
    /// </summary>
    public bool IsZero(int offset) => Check('\0', offset);

    public bool IsZero() => IsZero(0);

    /// <summary>
    ///     Check if the character at the specified position is tab.
    /// </summary>
    public bool IsTab(int offset) => Check('\t', offset);

    public bool IsTab() => IsTab(0);

    /// <summary>
    ///     Check if the character at the specified position is blank (space or tab).
    /// </summary>
    public bool IsBlank(int offset) => IsSpace(offset) || IsTab(offset);

    public bool IsBlank() => IsBlank(0);

    /// <summary>
    ///     Check if the character at the specified position is a line break.
    /// </summary>
    public bool IsBreak(int offset) => Check("\r\n\x85\x2028\x2029", offset);

    public bool IsBreak() => IsBreak(0);

    public bool IsCrLf(int offset) => Check('\r', offset) && Check('\n', offset + 1);

    public bool IsCrLf() => IsCrLf(0);

    /// <summary>
    ///     Check if the character is a line break or NUL.
    /// </summary>
    public bool IsBreakOrZero(int offset) => IsBreak(offset) || IsZero(offset);

    public bool IsBreakOrZero() => IsBreakOrZero(0);

    /// <summary>
    ///     Check if the character is a line break, space, tab, or NUL.
    /// </summary>
    public bool IsBlankOrBreakOrZero(int offset) => IsBlank(offset) || IsBreakOrZero(offset);

    public bool IsBlankOrBreakOrZero() => IsBlankOrBreakOrZero(0);

    public bool Check(char expected) => Check(expected, 0);

    public bool Check(char expected, int offset) => Buffer.Peek(offset) == expected;

    public bool Check(string expectedCharacters) => Check(expectedCharacters, 0);

    public bool Check(string expectedCharacters, int offset) {
        Debug.Assert(expectedCharacters.Length > 1, "Use Check(char, int) instead.");

        var character = Buffer.Peek(offset);

        foreach (var expected in expectedCharacters) {
            if (expected == character) {
                return true;
            }
        }

        return false;
    }
}
