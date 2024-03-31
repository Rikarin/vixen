namespace Rin.Core.Yaml;

/// <summary>
///     Represents a location inside a file
/// </summary>
public struct Mark {
    int index;
    int line;
    int column;

    /// <summary>
    ///     Gets a <see cref="Mark" /> with empty values.
    /// </summary>
    public static readonly Mark Empty;

    /// <summary>
    ///     Gets / sets the absolute offset in the file
    /// </summary>
    public int Index {
        get => index;
        set {
            if (value < 0) {
                throw new ArgumentOutOfRangeException(nameof(value), "Index must be greater than or equal to zero.");
            }

            index = value;
        }
    }

    /// <summary>
    ///     Gets / sets the number of the line
    /// </summary>
    public int Line {
        get => line;
        set {
            if (value < 0) {
                throw new ArgumentOutOfRangeException(nameof(value), "Line must be greater than or equal to zero.");
            }

            line = value;
        }
    }

    /// <summary>
    ///     Gets / sets the index of the column
    /// </summary>
    public int Column {
        get => column;
        set {
            if (value < 0) {
                throw new ArgumentOutOfRangeException(nameof(value), "Column must be greater than or equal to zero.");
            }

            column = value;
        }
    }

    /// <summary>
    ///     Returns a <see cref="System.String" /> that represents this instance.
    /// </summary>
    /// <returns>
    ///     A <see cref="System.String" /> that represents this instance.
    /// </returns>
    public override string ToString() => $"Lin: {line}, Col: {column}, Chr: {index}";
}
