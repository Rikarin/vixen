using System.Globalization;
using System.Text.RegularExpressions;

namespace Rin.Core.Yaml.Tokens;

/// <summary>
///     Represents a tag directive token.
/// </summary>
public class TagDirective : Token {
    static readonly Regex tagHandleValidator = new(@"^!([0-9A-Za-z_\-]*!)?$");

    /// <summary>
    ///     Gets the handle.
    /// </summary>
    /// <value>The handle.</value>
    public string Handle { get; }

    /// <summary>
    ///     Gets the prefix.
    /// </summary>
    /// <value>The prefix.</value>
    public string Prefix { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="TagDirective" /> class.
    /// </summary>
    /// <param name="handle">The handle.</param>
    /// <param name="prefix">The prefix.</param>
    public TagDirective(string handle, string prefix) : this(handle, prefix, Mark.Empty, Mark.Empty) { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="TagDirective" /> class.
    /// </summary>
    /// <param name="handle">The handle.</param>
    /// <param name="prefix">The prefix.</param>
    /// <param name="start">The start position of the token.</param>
    /// <param name="end">The end position of the token.</param>
    public TagDirective(string handle, string prefix, Mark start, Mark end) : base(start, end) {
        if (string.IsNullOrEmpty(handle)) {
            throw new ArgumentNullException(nameof(handle), "Tag handle must not be empty.");
        }

        if (!tagHandleValidator.IsMatch(handle)) {
            throw new ArgumentException(
                "Tag handle must start and end with '!' and contain alphanumerical characters only.",
                nameof(handle)
            );
        }

        if (string.IsNullOrEmpty(prefix)) {
            throw new ArgumentNullException(nameof(prefix), "Tag prefix must not be empty.");
        }

        Handle = handle;
        Prefix = prefix;
    }

    /// <summary>
    ///     Determines whether the specified System.Object is equal to the current System.Object.
    /// </summary>
    /// <param name="obj">The System.Object to compare with the current System.Object.</param>
    /// <returns>
    ///     true if the specified System.Object is equal to the current System.Object; otherwise, false.
    /// </returns>
    public override bool Equals(object? obj) {
        var other = obj as TagDirective;
        return other != null && Handle.Equals(other.Handle) && Prefix.Equals(other.Prefix);
    }

    /// <summary>
    ///     Serves as a hash function for a particular type.
    /// </summary>
    /// <returns>
    ///     A hash code for the current <see cref="T:System.Object" />.
    /// </returns>
    public override int GetHashCode() => Handle.GetHashCode() ^ Prefix.GetHashCode();

    /// <summary />
    public override string ToString() => string.Format(CultureInfo.InvariantCulture, "{0} => {1}", Handle, Prefix);
}
