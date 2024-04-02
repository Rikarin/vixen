using System.Globalization;

namespace Vixen.Core.Yaml.Events;

/// <summary>
///     Represents a document end event.
/// </summary>
public class DocumentEnd : ParsingEvent {
    /// <summary>
    ///     Gets a value indicating the variation of depth caused by this event.
    ///     The value can be either -1, 0 or 1. For start events, it will be 1,
    ///     for end events, it will be -1, and for the remaining events, it will be 0.
    /// </summary>
    public override int NestingIncrease => -1;

    /// <summary>
    ///     Gets a value indicating whether this instance is implicit.
    /// </summary>
    /// <value>
    ///     <c>true</c> if this instance is implicit; otherwise, <c>false</c>.
    /// </value>
    public bool IsImplicit { get; }

    /// <summary>
    ///     Gets the event type, which allows for simpler type comparisons.
    /// </summary>
    internal override EventType Type => EventType.YamlDocumentEndEvent;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DocumentEnd" /> class.
    /// </summary>
    /// <param name="isImplicit">Indicates whether the event is implicit.</param>
    /// <param name="start">The start position of the event.</param>
    /// <param name="end">The end position of the event.</param>
    public DocumentEnd(bool isImplicit, Mark start, Mark end) : base(start, end) {
        IsImplicit = isImplicit;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DocumentEnd" /> class.
    /// </summary>
    /// <param name="isImplicit">Indicates whether the event is implicit.</param>
    public DocumentEnd(bool isImplicit) : this(isImplicit, Mark.Empty, Mark.Empty) { }

    /// <summary>
    ///     Returns a <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
    /// </summary>
    /// <returns>
    ///     A <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
    /// </returns>
    public override string ToString() =>
        string.Format(
            CultureInfo.InvariantCulture,
            "Document end [isImplicit = {0}]",
            IsImplicit
        );
}
