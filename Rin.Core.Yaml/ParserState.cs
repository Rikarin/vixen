namespace Rin.Core.Yaml;

/// <summary>
///     Defines the YAML parser's state.
/// </summary>
enum ParserState {
    /// <summary>
    ///     Expect STREAM-START.
    /// </summary>
    YamlParseStreamStartState,

    /// <summary>
    ///     Expect the beginning of an implicit document.
    /// </summary>
    YamlParseImplicitDocumentStartState,

    /// <summary>
    ///     Expect DOCUMENT-START.
    /// </summary>
    YamlParseDocumentStartState,

    /// <summary>
    ///     Expect the content of a document.
    /// </summary>
    YamlParseDocumentContentState,

    /// <summary>
    ///     Expect DOCUMENT-END.
    /// </summary>
    YamlParseDocumentEndState,

    /// <summary>
    ///     Expect a block node.
    /// </summary>
    YamlParseBlockNodeState,

    /// <summary>
    ///     Expect a block node or indentless sequence.
    /// </summary>
    YamlParseBlockNodeOrIndentlessSequenceState,

    /// <summary>
    ///     Expect a flow node.
    /// </summary>
    YamlParseFlowNodeState,

    /// <summary>
    ///     Expect the first entry of a block sequence.
    /// </summary>
    YamlParseBlockSequenceFirstEntryState,

    /// <summary>
    ///     Expect an entry of a block sequence.
    /// </summary>
    YamlParseBlockSequenceEntryState,

    /// <summary>
    ///     Expect an entry of an indentless sequence.
    /// </summary>
    YamlParseIndentlessSequenceEntryState,

    /// <summary>
    ///     Expect the first key of a block mapping.
    /// </summary>
    YamlParseBlockMappingFirstKeyState,

    /// <summary>
    ///     Expect a block mapping key.
    /// </summary>
    YamlParseBlockMappingKeyState,

    /// <summary>
    ///     Expect a block mapping value.
    /// </summary>
    YamlParseBlockMappingValueState,

    /// <summary>
    ///     Expect the first entry of a flow sequence.
    /// </summary>
    YamlParseFlowSequenceFirstEntryState,

    /// <summary>
    ///     Expect an entry of a flow sequence.
    /// </summary>
    YamlParseFlowSequenceEntryState,

    /// <summary>
    ///     Expect a key of an ordered mapping.
    /// </summary>
    YamlParseFlowSequenceEntryMappingKeyState,

    /// <summary>
    ///     Expect a value of an ordered mapping.
    /// </summary>
    YamlParseFlowSequenceEntryMappingValueState,

    /// <summary>
    ///     Expect the and of an ordered mapping entry.
    /// </summary>
    YamlParseFlowSequenceEntryMappingEndState,

    /// <summary>
    ///     Expect the first key of a flow mapping.
    /// </summary>
    YamlParseFlowMappingFirstKeyState,

    /// <summary>
    ///     Expect a key of a flow mapping.
    /// </summary>
    YamlParseFlowMappingKeyState,

    /// <summary>
    ///     Expect a value of a flow mapping.
    /// </summary>
    YamlParseFlowMappingValueState,

    /// <summary>
    ///     Expect an empty value of a flow mapping.
    /// </summary>
    YamlParseFlowMappingEmptyValueState,

    /// <summary>
    ///     Expect nothing.
    /// </summary>
    YamlParseEndState
}
