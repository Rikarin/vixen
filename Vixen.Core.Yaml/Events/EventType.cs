namespace Vixen.Core.Yaml.Events;

/// <summary>
///     Defines the event types. This allows for simpler type comparisons.
/// </summary>
enum EventType {
    /// <summary>
    ///     An empty event.
    /// </summary>
    YamlNoEvent,

    /// <summary>
    ///     A STREAM-START event.
    /// </summary>
    YamlStreamStartEvent,

    /// <summary>
    ///     A STREAM-END event.
    /// </summary>
    YamlStreamEndEvent,

    /// <summary>
    ///     A DOCUMENT-START event.
    /// </summary>
    YamlDocumentStartEvent,

    /// <summary>
    ///     A DOCUMENT-END event.
    /// </summary>
    YamlDocumentEndEvent,

    /// <summary>
    ///     An ALIAS event.
    /// </summary>
    YamlAliasEvent,

    /// <summary>
    ///     A SCALAR event.
    /// </summary>
    YamlScalarEvent,

    /// <summary>
    ///     A SEQUENCE-START event.
    /// </summary>
    YamlSequenceStartEvent,

    /// <summary>
    ///     A SEQUENCE-END event.
    /// </summary>
    YamlSequenceEndEvent,

    /// <summary>
    ///     A MAPPING-START event.
    /// </summary>
    YamlMappingStartEvent,

    /// <summary>
    ///     A MAPPING-END event.
    /// </summary>
    YamlMappingEndEvent
}
