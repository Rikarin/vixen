using System.Text.RegularExpressions;

namespace Rin.Core.Yaml.Events;

/// <summary>
///     Contains the behavior that is common between node events.
/// </summary>
public abstract class NodeEvent : ParsingEvent {
    internal static readonly Regex anchorValidator = new(@"^[0-9a-zA-Z_\-]+$");

    /// <summary>
    ///     Gets the anchor.
    /// </summary>
    /// <value></value>
    public string Anchor { get; }

    /// <summary>
    ///     Gets the tag.
    /// </summary>
    /// <value></value>
    public string Tag { get; }

    /// <summary>
    ///     Gets a value indicating whether this instance is canonical.
    /// </summary>
    /// <value></value>
    public abstract bool IsCanonical { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="NodeEvent" /> class.
    /// </summary>
    /// <param name="anchor">The anchor.</param>
    /// <param name="tag">The tag.</param>
    /// <param name="start">The start position of the event.</param>
    /// <param name="end">The end position of the event.</param>
    protected NodeEvent(string anchor, string tag, Mark start, Mark end) : base(start, end) {
        if (anchor != null) {
            if (anchor.Length == 0) {
                throw new ArgumentException("Anchor value must not be empty.", nameof(anchor));
            }

            if (!anchorValidator.IsMatch(anchor)) {
                throw new ArgumentException(
                    "Anchor value must contain alphanumerical characters only.",
                    nameof(anchor)
                );
            }
        }

        if (tag is { Length: 0 }) {
            throw new ArgumentException("Tag value must not be empty.", nameof(tag));
        }

        Anchor = anchor;
        Tag = tag;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="NodeEvent" /> class.
    /// </summary>
    protected NodeEvent(string anchor, string tag) : this(anchor, tag, Mark.Empty, Mark.Empty) { }
}
