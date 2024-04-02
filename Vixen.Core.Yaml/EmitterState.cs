namespace Vixen.Core.Yaml;

/// <summary>
///     Defines the YAML emitter's state.
/// </summary>
enum EmitterState {
    /// <summary>
    ///     Expect STREAM-START.
    /// </summary>
    YamlEmitStreamStartState,

    /// <summary>
    ///     Expect the first DOCUMENT-START or STREAM-END.
    /// </summary>
    YamlEmitFirstDocumentStartState,

    /// <summary>
    ///     Expect DOCUMENT-START or STREAM-END.
    /// </summary>
    YamlEmitDocumentStartState,

    /// <summary>
    ///     Expect the content of a document.
    /// </summary>
    YamlEmitDocumentContentState,

    /// <summary>
    ///     Expect DOCUMENT-END.
    /// </summary>
    YamlEmitDocumentEndState,

    /// <summary>
    ///     Expect the first item of a flow sequence.
    /// </summary>
    YamlEmitFlowSequenceFirstItemState,

    /// <summary>
    ///     Expect an item of a flow sequence.
    /// </summary>
    YamlEmitFlowSequenceItemState,

    /// <summary>
    ///     Expect the first key of a flow mapping.
    /// </summary>
    YamlEmitFlowMappingFirstKeyState,

    /// <summary>
    ///     Expect a key of a flow mapping.
    /// </summary>
    YamlEmitFlowMappingKeyState,

    /// <summary>
    ///     Expect a value for a simple key of a flow mapping.
    /// </summary>
    YamlEmitFlowMappingSimpleValueState,

    /// <summary>
    ///     Expect a value of a flow mapping.
    /// </summary>
    YamlEmitFlowMappingValueState,

    /// <summary>
    ///     Expect the first item of a block sequence.
    /// </summary>
    YamlEmitBlockSequenceFirstItemState,

    /// <summary>
    ///     Expect an item of a block sequence.
    /// </summary>
    YamlEmitBlockSequenceItemState,

    /// <summary>
    ///     Expect the first key of a block mapping.
    /// </summary>
    YamlEmitBlockMappingFirstKeyState,

    /// <summary>
    ///     Expect the key of a block mapping.
    /// </summary>
    YamlEmitBlockMappingKeyState,

    /// <summary>
    ///     Expect a value for a simple key of a block mapping.
    /// </summary>
    YamlEmitBlockMappingSimpleValueState,

    /// <summary>
    ///     Expect a value of a block mapping.
    /// </summary>
    YamlEmitBlockMappingValueState,

    /// <summary>
    ///     Expect nothing.
    /// </summary>
    YamlEmitEndState
}
