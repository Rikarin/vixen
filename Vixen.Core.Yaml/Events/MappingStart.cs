using System.Globalization;

namespace Vixen.Core.Yaml.Events;

/// <summary>
///     Represents a mapping start event.
/// </summary>
public class MappingStart : NodeEvent {
    /// <summary>
    ///     Gets a value indicating the variation of depth caused by this event.
    ///     The value can be either -1, 0 or 1. For start events, it will be 1,
    ///     for end events, it will be -1, and for the remaining events, it will be 0.
    /// </summary>
    public override int NestingIncrease => 1;

    /// <summary>
    ///     Gets a value indicating whether this instance is implicit.
    /// </summary>
    /// <value>
    ///     <c>true</c> if this instance is implicit; otherwise, <c>false</c>.
    /// </value>
    public bool IsImplicit { get; }

    /// <summary>
    ///     Gets a value indicating whether this instance is canonical.
    /// </summary>
    /// <value></value>
    public override bool IsCanonical => !IsImplicit;

    /// <summary>
    ///     Gets the style of the mapping.
    /// </summary>
    public DataStyle Style { get; }

    /// <summary>
    ///     Gets the event type, which allows for simpler type comparisons.
    /// </summary>
    internal override EventType Type => EventType.YamlMappingStartEvent;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MappingStart" /> class.
    /// </summary>
    /// <param name="anchor">The anchor.</param>
    /// <param name="tag">The tag.</param>
    /// <param name="isImplicit">Indicates whether the event is implicit.</param>
    /// <param name="style">The style of the mapping.</param>
    /// <param name="start">The start position of the event.</param>
    /// <param name="end">The end position of the event.</param>
    public MappingStart(string anchor, string tag, bool isImplicit, DataStyle style, Mark start, Mark end)
        : base(anchor, tag, start, end) {
        IsImplicit = isImplicit;
        Style = style;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MappingStart" /> class.
    /// </summary>
    /// <param name="anchor">The anchor.</param>
    /// <param name="tag">The tag.</param>
    /// <param name="isImplicit">Indicates whether the event is implicit.</param>
    /// <param name="style">The style of the mapping.</param>
    public MappingStart(string anchor, string tag, bool isImplicit, DataStyle style)
        : this(anchor, tag, isImplicit, style, Mark.Empty, Mark.Empty) { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MappingStart" /> class.
    /// </summary>
    public MappingStart() : this(null, null, true, DataStyle.Any, Mark.Empty, Mark.Empty) { }

    /// <summary>
    ///     Returns a <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
    /// </summary>
    /// <returns>
    ///     A <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
    /// </returns>
    public override string ToString() =>
        string.Format(
            CultureInfo.InvariantCulture,
            "Mapping start [anchor = {0}, tag = {1}, isImplicit = {2}, style = {3}]",
            Anchor,
            Tag,
            IsImplicit,
            Style
        );
}
