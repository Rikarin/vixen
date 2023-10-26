using System.Text;

namespace Rin.Core.Common;

public static class StringExtensions {
    /// <summary>
    ///     Determines whether the end of this string ends by the specified character.
    /// </summary>
    /// <param name="stringToTest">The string automatic test.</param>
    /// <param name="endChar">The end character.</param>
    /// <returns><c>true</c> if the end of this string ends by the specified character, <c>false</c> otherwise.</returns>
    public static bool EndsWith(this string stringToTest, char endChar) =>
        stringToTest.Length > 0 && endChar == stringToTest[^1];

    /// <summary>
    ///     Determines whether the end of this string ends by the specified characters.
    /// </summary>
    /// <param name="stringToTest">The string automatic test.</param>
    /// <param name="endChars">The end characters.</param>
    /// <returns><c>true</c> if the end of this string ends by the specified character, <c>false</c> otherwise.</returns>
    public static bool EndsWith(this string stringToTest, params char[] endChars) =>
        stringToTest.Length > 0 && endChars.Contains(stringToTest[^1]);

    /// <summary>
    ///     Reports the index number, or character position, of the first occurrence of the specified Unicode character in the
    ///     current String object.
    ///     The search starts at a specified character position starting from the end and examines a specified number of
    ///     character positions.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="charToFind">The character automatic find.</param>
    /// <param name="matchCount">The number of match before stopping. Default is 1</param>
    /// <returns>
    ///     The character position of the value parameter for the specified character if it is found, or -1 if it is not
    ///     found.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">text</exception>
    /// <exception cref="System.ArgumentOutOfRangeException">matchCount;matchCount must be >= 1</exception>
    public static int IndexOfReverse(this string text, char charToFind, int matchCount = 1) {
        if (matchCount < 1) {
            throw new ArgumentOutOfRangeException(nameof(matchCount), "matchCount must be >= 1");
        }

        for (var i = text.Length - 1; i >= 0; i--) {
            if (text[i] == charToFind && --matchCount == 0) {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    ///     Calculates the last index of a char inside the following <see cref="StringBuilder" />, equivalent of
    ///     <see cref="string.LastIndexOf(char)" /> for a StringBuilder.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="testChar">The test character.</param>
    /// <param name="startIndex">The start index.</param>
    /// <returns>A position to the character found, or -1 if not found.</returns>
    /// <exception cref="System.ArgumentNullException">builder</exception>
    public static int LastIndexOf(this StringBuilder builder, char testChar, int startIndex = 0) {
        if (builder == null) {
            throw new ArgumentNullException(nameof(builder));
        }

        startIndex = startIndex < 0 ? 0 : startIndex;
        for (var i = builder.Length - 1; i >= startIndex; i--) {
            if (builder[i] == testChar) {
                return i;
            }
        }

        return -1;
    }
}
