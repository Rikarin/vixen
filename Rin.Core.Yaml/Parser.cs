using Rin.Core.Yaml.Events;
using Rin.Core.Yaml.Tokens;
using System.Diagnostics;
using AnchorAlias = Rin.Core.Yaml.Tokens.AnchorAlias;
using DocumentEnd = Rin.Core.Yaml.Tokens.DocumentEnd;
using DocumentStart = Rin.Core.Yaml.Tokens.DocumentStart;
using Event = Rin.Core.Yaml.Events.ParsingEvent;
using Scalar = Rin.Core.Yaml.Tokens.Scalar;
using StreamEnd = Rin.Core.Yaml.Tokens.StreamEnd;
using StreamStart = Rin.Core.Yaml.Tokens.StreamStart;

namespace Rin.Core.Yaml;

/// <summary>
///     Parses YAML streams.
/// </summary>
public class Parser : IParser {
    readonly Stack<ParserState> states = new();
    readonly TagDirectiveCollection tagDirectives = new();
    ParserState state;
    readonly Scanner scanner;
    Token? currentToken;

    /// <summary>
    ///     Gets the current event.
    /// </summary>
    public Event? Current { get; private set; }

    /// <inheritdoc />
    public bool IsEndOfStream => state == ParserState.YamlParseEndState;

    /// <summary>
    ///     Initializes a new instance of the <see cref="IParser" /> class.
    /// </summary>
    /// <param name="input">The input where the YAML stream is to be read.</param>
    public Parser(TextReader input) {
        scanner = new(input);
    }

    /// <summary>
    ///     Moves to the next event.
    /// </summary>
    /// <returns>Returns true if there are more events available, otherwise returns false.</returns>
    public bool MoveNext() {
        // No events after the end of the stream or error.
        if (state == ParserState.YamlParseEndState) {
            Current = null;
            return false;
        }

        // Generate the next event.
        Current = StateMachine();
        return true;
    }

    Token GetCurrentToken() {
        if (currentToken == null) {
            if (scanner.InternalMoveNext()) {
                currentToken = scanner.Current;
            }
        }

        return currentToken;
    }

    Event StateMachine() {
        switch (state) {
            case ParserState.YamlParseStreamStartState:
                return ParseStreamStart();

            case ParserState.YamlParseImplicitDocumentStartState:
                return ParseDocumentStart(true);

            case ParserState.YamlParseDocumentStartState:
                return ParseDocumentStart(false);

            case ParserState.YamlParseDocumentContentState:
                return ParseDocumentContent();

            case ParserState.YamlParseDocumentEndState:
                return ParseDocumentEnd();

            case ParserState.YamlParseBlockNodeState:
                return ParseNode(true, false);

            case ParserState.YamlParseBlockNodeOrIndentlessSequenceState:
                return ParseNode(true, true);

            case ParserState.YamlParseFlowNodeState:
                return ParseNode(false, false);

            case ParserState.YamlParseBlockSequenceFirstEntryState:
                return ParseBlockSequenceEntry(true);

            case ParserState.YamlParseBlockSequenceEntryState:
                return ParseBlockSequenceEntry(false);

            case ParserState.YamlParseIndentlessSequenceEntryState:
                return ParseIndentlessSequenceEntry();

            case ParserState.YamlParseBlockMappingFirstKeyState:
                return ParseBlockMappingKey(true);

            case ParserState.YamlParseBlockMappingKeyState:
                return ParseBlockMappingKey(false);

            case ParserState.YamlParseBlockMappingValueState:
                return ParseBlockMappingValue();

            case ParserState.YamlParseFlowSequenceFirstEntryState:
                return ParseFlowSequenceEntry(true);

            case ParserState.YamlParseFlowSequenceEntryState:
                return ParseFlowSequenceEntry(false);

            case ParserState.YamlParseFlowSequenceEntryMappingKeyState:
                return ParseFlowSequenceEntryMappingKey();

            case ParserState.YamlParseFlowSequenceEntryMappingValueState:
                return ParseFlowSequenceEntryMappingValue();

            case ParserState.YamlParseFlowSequenceEntryMappingEndState:
                return ParseFlowSequenceEntryMappingEnd();

            case ParserState.YamlParseFlowMappingFirstKeyState:
                return ParseFlowMappingKey(true);

            case ParserState.YamlParseFlowMappingKeyState:
                return ParseFlowMappingKey(false);

            case ParserState.YamlParseFlowMappingValueState:
                return ParseFlowMappingValue(false);

            case ParserState.YamlParseFlowMappingEmptyValueState:
                return ParseFlowMappingValue(true);

            default:
                Debug.Assert(false, "Invalid state"); // Invalid state.
                throw new InvalidOperationException();
        }
    }

    void Skip() {
        if (currentToken != null) {
            currentToken = null;
            scanner.ConsumeCurrent();
        }
    }

    /// <summary>
    ///     Parse the production:
    ///     stream   ::= STREAM-START implicit_document? explicit_document* STREAM-END
    ///     ************
    /// </summary>
    Event ParseStreamStart() {
        var streamStart = GetCurrentToken() as StreamStart;
        if (streamStart == null) {
            var current = GetCurrentToken();
            throw new SemanticErrorException(current.Start, current.End, "Did not find expected <stream-start>.");
        }

        Skip();

        state = ParserState.YamlParseImplicitDocumentStartState;
        return new Events.StreamStart(streamStart.Start, streamStart.End);
    }

    /// <summary>
    ///     Parse the productions:
    ///     implicit_document    ::= block_node DOCUMENT-END*
    ///     *
    ///     explicit_document    ::= DIRECTIVE* DOCUMENT-START block_node? DOCUMENT-END*
    ///     *************************
    /// </summary>
    Event ParseDocumentStart(bool isImplicit) {
        // Parse extra document end indicators.
        if (!isImplicit) {
            while (GetCurrentToken() is DocumentEnd) {
                Skip();
            }
        }

        // Parse an isImplicit document.
        if (
            isImplicit
            && !(GetCurrentToken() is VersionDirective
                || GetCurrentToken() is TagDirective
                || GetCurrentToken() is DocumentStart
                || GetCurrentToken() is StreamEnd)
        ) {
            var directives = new TagDirectiveCollection();
            ProcessDirectives(directives);

            states.Push(ParserState.YamlParseDocumentEndState);
            state = ParserState.YamlParseBlockNodeState;
            
            return new Events.DocumentStart(null, directives, true, GetCurrentToken().Start, GetCurrentToken().End);
        }

        // Parse an explicit document.
        if (GetCurrentToken() is not StreamEnd) {
            var start = GetCurrentToken().Start;
            var directives = new TagDirectiveCollection();
            var versionDirective = ProcessDirectives(directives);

            var current = GetCurrentToken();
            if (current is not DocumentStart) {
                throw new SemanticErrorException(current.Start, current.End, "Did not find expected <document start>.");
            }

            states.Push(ParserState.YamlParseDocumentEndState);
            state = ParserState.YamlParseDocumentContentState;

            Event evt = new Events.DocumentStart(versionDirective, directives, false, start, current.End);
            Skip();
            return evt;
        }

        // Parse the stream end.
        else {
            state = ParserState.YamlParseEndState;

            Event evt = new Events.StreamEnd(GetCurrentToken().Start, GetCurrentToken().End);
            // Do not call skip here because that would throw an exception
            if (scanner.InternalMoveNext()) {
                throw new InvalidOperationException("The scanner should contain no more tokens.");
            }

            return evt;
        }
    }

    /// <summary>
    ///     Parse directives.
    /// </summary>
    VersionDirective ProcessDirectives(TagDirectiveCollection tags) {
        VersionDirective? version = null;

        while (true) {
            VersionDirective? currentVersion;
            TagDirective? tag;

            if ((currentVersion = GetCurrentToken() as VersionDirective) != null) {
                if (version != null) {
                    throw new SemanticErrorException(
                        currentVersion.Start,
                        currentVersion.End,
                        "Found duplicate %YAML directive."
                    );
                }

                if (
                    currentVersion.Version.Major != Constants.MajorVersion
                    || currentVersion.Version.Minor != Constants.MinorVersion
                ) {
                    throw new SemanticErrorException(
                        currentVersion.Start,
                        currentVersion.End,
                        "Found incompatible YAML document."
                    );
                }

                version = currentVersion;
            } else if ((tag = GetCurrentToken() as TagDirective) != null) {
                if (tagDirectives.Contains(tag.Handle)) {
                    throw new SemanticErrorException(tag.Start, tag.End, "Found duplicate %TAG directive.");
                }

                tagDirectives.Add(tag);
                tags?.Add(tag);
            } else {
                break;
            }

            Skip();
        }

        if (tags != null) {
            AddDefaultTagDirectives(tags);
        }

        AddDefaultTagDirectives(tagDirectives);
        return version!;
    }

    static void AddDefaultTagDirectives(TagDirectiveCollection directives) {
        foreach (var directive in Constants.DefaultTagDirectives) {
            if (!directives.Contains(directive)) {
                directives.Add(directive);
            }
        }
    }

    /// <summary>
    ///     Parse the productions:
    ///     explicit_document    ::= DIRECTIVE* DOCUMENT-START block_node? DOCUMENT-END*
    ///     ***********
    /// </summary>
    Event ParseDocumentContent() {
        if (
            GetCurrentToken() is VersionDirective
            || GetCurrentToken() is TagDirective
            || GetCurrentToken() is DocumentStart
            || GetCurrentToken() is DocumentEnd
            || GetCurrentToken() is StreamEnd
        ) {
            state = states.Pop();
            return ProcessEmptyScalar(scanner.CurrentPosition);
        }

        return ParseNode(true, false);
    }

    /// <summary>
    ///     Generate an empty scalar event.
    /// </summary>
    static Event ProcessEmptyScalar(Mark position) =>
        new Events.Scalar(null, null, string.Empty, ScalarStyle.Plain, true, false, position, position);

    /// <summary>
    ///     Parse the productions:
    ///     block_node_or_indentless_sequence    ::=
    ///     ALIAS
    ///     *****
    ///     | properties (block_content | indentless_block_sequence)?
    ///     **********  *
    ///     | block_content | indentless_block_sequence
    ///     *
    ///     block_node           ::= ALIAS
    ///     *****
    ///     | properties block_content?
    ///     ********** *
    ///     | block_content
    ///     *
    ///     flow_node            ::= ALIAS
    ///     *****
    ///     | properties flow_content?
    ///     ********** *
    ///     | flow_content
    ///     *
    ///     properties           ::= TAG ANCHOR? | ANCHOR TAG?
    ///     *************************
    ///     block_content        ::= block_collection | flow_collection | SCALAR
    ///     ******
    ///     flow_content         ::= flow_collection | SCALAR
    ///     ******
    /// </summary>
    Event ParseNode(bool isBlock, bool isIndentlessSequence) {
        if (GetCurrentToken() is AnchorAlias alias) {
            state = states.Pop();
            Event evt = new Events.AnchorAlias(alias.Value, alias.Start, alias.End);
            Skip();
            return evt;
        }

        var start = GetCurrentToken().Start;

        Anchor? anchor = null;
        Tag? tag = null;

        // The anchor and the tag can be in any order. This loop repeats at most twice.
        while (true) {
            if (anchor == null && (anchor = GetCurrentToken() as Anchor) != null) {
                Skip();
            } else if (tag == null && (tag = GetCurrentToken() as Tag) != null) {
                Skip();
            } else {
                break;
            }
        }

        string? tagName = null;
        if (tag != null) {
            if (string.IsNullOrEmpty(tag.Handle)) {
                tagName = tag.Suffix;
            } else if (tagDirectives.Contains(tag.Handle)) {
                tagName = string.Concat(tagDirectives[tag.Handle].Prefix, tag.Suffix);
            } else {
                throw new SemanticErrorException(
                    tag.Start,
                    tag.End,
                    "While parsing a node, find undefined tag handle."
                );
            }
        }

        if (string.IsNullOrEmpty(tagName)) {
            tagName = null;
        }

        var anchorName = anchor != null ? string.IsNullOrEmpty(anchor.Value) ? null : anchor.Value : null;
        var isImplicit = string.IsNullOrEmpty(tagName);

        if (isIndentlessSequence && GetCurrentToken() is BlockEntry) {
            state = ParserState.YamlParseIndentlessSequenceEntryState;

            return new SequenceStart(
                anchorName,
                tagName,
                isImplicit,
                DataStyle.Normal,
                start,
                GetCurrentToken().End
            );
        }

        if (GetCurrentToken() is Scalar scalar) {
            var isPlainImplicit = false;
            var isQuotedImplicit = false;
            if ((scalar.Style == ScalarStyle.Plain && tagName == null) || tagName == Constants.DefaultHandle) {
                isPlainImplicit = true;
            } else if (tagName == null) {
                isQuotedImplicit = true;
            }

            state = states.Pop();
            Event evt = new Events.Scalar(
                anchorName,
                tagName,
                scalar.Value,
                scalar.Style,
                isPlainImplicit,
                isQuotedImplicit,
                start,
                scalar.End
            );

            Skip();
            return evt;
        }

        if (GetCurrentToken() is FlowSequenceStart flowSequenceStart) {
            state = ParserState.YamlParseFlowSequenceFirstEntryState;
            return new SequenceStart(anchorName, tagName, isImplicit, DataStyle.Compact, start, flowSequenceStart.End);
        }

        if (GetCurrentToken() is FlowMappingStart flowMappingStart) {
            state = ParserState.YamlParseFlowMappingFirstKeyState;
            return new MappingStart(anchorName, tagName, isImplicit, DataStyle.Compact, start, flowMappingStart.End);
        }

        if (isBlock) {
            if (GetCurrentToken() is BlockSequenceStart blockSequenceStart) {
                state = ParserState.YamlParseBlockSequenceFirstEntryState;
                return new SequenceStart(
                    anchorName,
                    tagName,
                    isImplicit,
                    DataStyle.Normal,
                    start,
                    blockSequenceStart.End
                );
            }

            if (GetCurrentToken() is BlockMappingStart) {
                state = ParserState.YamlParseBlockMappingFirstKeyState;
                return new MappingStart(
                    anchorName,
                    tagName,
                    isImplicit,
                    DataStyle.Normal,
                    start,
                    GetCurrentToken().End
                );
            }
        }

        if (anchorName != null || tag != null) {
            state = states.Pop();
            return new Events.Scalar(
                anchorName,
                tagName,
                string.Empty,
                ScalarStyle.Plain,
                isImplicit,
                false,
                start,
                GetCurrentToken().End
            );
        }

        var current = GetCurrentToken();
        throw new SemanticErrorException(
            current.Start,
            current.End,
            "While parsing a node, did not find expected node content."
        );
    }

    /// <summary>
    ///     Parse the productions:
    ///     implicit_document    ::= block_node DOCUMENT-END*
    ///     *************
    ///     explicit_document    ::= DIRECTIVE* DOCUMENT-START block_node? DOCUMENT-END*
    ///     *************
    /// </summary>
    Event ParseDocumentEnd() {
        var isImplicit = true;
        var start = GetCurrentToken().Start;
        var end = start;

        if (GetCurrentToken() is DocumentEnd) {
            end = GetCurrentToken().End;
            Skip();
            isImplicit = false;
        }

        tagDirectives.Clear();

        state = ParserState.YamlParseDocumentStartState;
        return new Events.DocumentEnd(isImplicit, start, end);
    }

    /// <summary>
    ///     Parse the productions:
    ///     block_sequence ::= BLOCK-SEQUENCE-START (BLOCK-ENTRY block_node?)* BLOCK-END
    ///     ********************  *********** *             *********
    /// </summary>
    Event ParseBlockSequenceEntry(bool isFirst) {
        if (isFirst) {
            GetCurrentToken();
            Skip();
        }

        if (GetCurrentToken() is BlockEntry) {
            var mark = GetCurrentToken().End;

            Skip();
            if (!(GetCurrentToken() is BlockEntry || GetCurrentToken() is BlockEnd)) {
                states.Push(ParserState.YamlParseBlockSequenceEntryState);
                return ParseNode(true, false);
            }

            state = ParserState.YamlParseBlockSequenceEntryState;
            return ProcessEmptyScalar(mark);
        }

        if (GetCurrentToken() is BlockEnd) {
            state = states.Pop();
            Event evt = new SequenceEnd(GetCurrentToken().Start, GetCurrentToken().End);
            Skip();
            return evt;
        }

        var current = GetCurrentToken();
        throw new SemanticErrorException(
            current.Start,
            current.End,
            "While parsing a block collection, did not find expected '-' indicator."
        );
    }

    /// <summary>
    ///     Parse the productions:
    ///     indentless_sequence  ::= (BLOCK-ENTRY block_node?)+
    ///     *********** *
    /// </summary>
    Event ParseIndentlessSequenceEntry() {
        if (GetCurrentToken() is BlockEntry) {
            var mark = GetCurrentToken().End;
            Skip();

            if (!(GetCurrentToken() is BlockEntry
                    || GetCurrentToken() is Key
                    || GetCurrentToken() is Value
                    || GetCurrentToken() is BlockEnd)) {
                states.Push(ParserState.YamlParseIndentlessSequenceEntryState);
                return ParseNode(true, false);
            }

            state = ParserState.YamlParseIndentlessSequenceEntryState;
            return ProcessEmptyScalar(mark);
        }

        state = states.Pop();
        return new SequenceEnd(GetCurrentToken().Start, GetCurrentToken().End);
    }

    /// <summary>
    ///     Parse the productions:
    ///     block_mapping        ::= BLOCK-MAPPING_START
    ///     *******************
    ///     ((KEY block_node_or_indentless_sequence?)?
    ///     *** *
    ///     (VALUE block_node_or_indentless_sequence?)?)*
    ///     BLOCK-END
    ///     *********
    /// </summary>
    Event ParseBlockMappingKey(bool isFirst) {
        if (isFirst) {
            GetCurrentToken();
            Skip();
        }

        if (GetCurrentToken() is Key) {
            var mark = GetCurrentToken().End;
            Skip();
            if (!(GetCurrentToken() is Key || GetCurrentToken() is Value || GetCurrentToken() is BlockEnd)) {
                states.Push(ParserState.YamlParseBlockMappingValueState);
                return ParseNode(true, true);
            }

            state = ParserState.YamlParseBlockMappingValueState;
            return ProcessEmptyScalar(mark);
        }

        if (GetCurrentToken() is BlockEnd) {
            state = states.Pop();
            Event evt = new MappingEnd(GetCurrentToken().Start, GetCurrentToken().End);
            Skip();
            return evt;
        }

        var current = GetCurrentToken();
        throw new SemanticErrorException(
            current.Start,
            current.End,
            "While parsing a block mapping, did not find expected key."
        );
    }

    /// <summary>
    ///     Parse the productions:
    ///     block_mapping        ::= BLOCK-MAPPING_START
    ///     ((KEY block_node_or_indentless_sequence?)?
    ///     (VALUE block_node_or_indentless_sequence?)?)*
    ///     ***** *
    ///     BLOCK-END
    /// </summary>
    Event ParseBlockMappingValue() {
        if (GetCurrentToken() is Value) {
            var mark = GetCurrentToken().End;
            Skip();

            if (!(GetCurrentToken() is Key || GetCurrentToken() is Value || GetCurrentToken() is BlockEnd)) {
                states.Push(ParserState.YamlParseBlockMappingKeyState);
                return ParseNode(true, true);
            }

            state = ParserState.YamlParseBlockMappingKeyState;
            return ProcessEmptyScalar(mark);
        }

        state = ParserState.YamlParseBlockMappingKeyState;
        return ProcessEmptyScalar(GetCurrentToken().Start);
    }

    /// <summary>
    ///     Parse the productions:
    ///     flow_sequence        ::= FLOW-SEQUENCE-START
    ///     *******************
    ///     (flow_sequence_entry FLOW-ENTRY)*
    ///     *                   **********
    ///     flow_sequence_entry?
    ///     *
    ///     FLOW-SEQUENCE-END
    ///     *****************
    ///     flow_sequence_entry  ::= flow_node | KEY flow_node? (VALUE flow_node?)?
    ///     *
    /// </summary>
    Event ParseFlowSequenceEntry(bool isFirst) {
        if (isFirst) {
            GetCurrentToken();
            Skip();
        }

        Event evt;
        if (!(GetCurrentToken() is FlowSequenceEnd)) {
            if (!isFirst) {
                if (GetCurrentToken() is FlowEntry) {
                    Skip();
                } else {
                    var current = GetCurrentToken();
                    throw new SemanticErrorException(
                        current.Start,
                        current.End,
                        "While parsing a flow sequence, did not find expected ',' or ']'."
                    );
                }
            }

            if (GetCurrentToken() is Key) {
                state = ParserState.YamlParseFlowSequenceEntryMappingKeyState;
                evt = new MappingStart(null, null, true, DataStyle.Compact);
                Skip();
                return evt;
            }

            if (!(GetCurrentToken() is FlowSequenceEnd)) {
                states.Push(ParserState.YamlParseFlowSequenceEntryState);
                return ParseNode(false, false);
            }
        }

        state = states.Pop();
        evt = new SequenceEnd(GetCurrentToken().Start, GetCurrentToken().End);
        Skip();
        return evt;
    }

    /// <summary>
    ///     Parse the productions:
    ///     flow_sequence_entry  ::= flow_node | KEY flow_node? (VALUE flow_node?)?
    ///     *** *
    /// </summary>
    Event ParseFlowSequenceEntryMappingKey() {
        if (!(GetCurrentToken() is Value || GetCurrentToken() is FlowEntry || GetCurrentToken() is FlowSequenceEnd)) {
            states.Push(ParserState.YamlParseFlowSequenceEntryMappingValueState);
            return ParseNode(false, false);
        }

        var mark = GetCurrentToken().End;
        Skip();
        state = ParserState.YamlParseFlowSequenceEntryMappingValueState;
        return ProcessEmptyScalar(mark);
    }

    /// <summary>
    ///     Parse the productions:
    ///     flow_sequence_entry  ::= flow_node | KEY flow_node? (VALUE flow_node?)?
    ///     ***** *
    /// </summary>
    Event ParseFlowSequenceEntryMappingValue() {
        if (GetCurrentToken() is Value) {
            Skip();
            if (!(GetCurrentToken() is FlowEntry || GetCurrentToken() is FlowSequenceEnd)) {
                states.Push(ParserState.YamlParseFlowSequenceEntryMappingEndState);
                return ParseNode(false, false);
            }
        }

        state = ParserState.YamlParseFlowSequenceEntryMappingEndState;
        return ProcessEmptyScalar(GetCurrentToken().Start);
    }

    /// <summary>
    ///     Parse the productions:
    ///     flow_sequence_entry  ::= flow_node | KEY flow_node? (VALUE flow_node?)?
    ///     *
    /// </summary>
    Event ParseFlowSequenceEntryMappingEnd() {
        state = ParserState.YamlParseFlowSequenceEntryState;
        return new MappingEnd(GetCurrentToken().Start, GetCurrentToken().End);
    }

    /// <summary>
    ///     Parse the productions:
    ///     flow_mapping         ::= FLOW-MAPPING-START
    ///     ******************
    ///     (flow_mapping_entry FLOW-ENTRY)*
    ///     *                  **********
    ///     flow_mapping_entry?
    ///     ******************
    ///     FLOW-MAPPING-END
    ///     ****************
    ///     flow_mapping_entry   ::= flow_node | KEY flow_node? (VALUE flow_node?)?
    ///     *           *** *
    /// </summary>
    Event ParseFlowMappingKey(bool isFirst) {
        if (isFirst) {
            GetCurrentToken();
            Skip();
        }

        if (GetCurrentToken() is not FlowMappingEnd) {
            if (!isFirst) {
                if (GetCurrentToken() is FlowEntry) {
                    Skip();
                } else {
                    var current = GetCurrentToken();
                    throw new SemanticErrorException(
                        current.Start,
                        current.End,
                        "While parsing a flow mapping,  did not find expected ',' or '}'."
                    );
                }
            }

            if (GetCurrentToken() is Key) {
                Skip();

                if (!(GetCurrentToken() is Value
                        || GetCurrentToken() is FlowEntry
                        || GetCurrentToken() is FlowMappingEnd)) {
                    states.Push(ParserState.YamlParseFlowMappingValueState);
                    return ParseNode(false, false);
                }

                state = ParserState.YamlParseFlowMappingValueState;
                return ProcessEmptyScalar(GetCurrentToken().Start);
            }

            if (!(GetCurrentToken() is FlowMappingEnd)) {
                states.Push(ParserState.YamlParseFlowMappingEmptyValueState);
                return ParseNode(false, false);
            }
        }

        state = states.Pop();
        Event evt = new MappingEnd(GetCurrentToken().Start, GetCurrentToken().End);
        Skip();
        return evt;
    }

    /// <summary>
    ///     Parse the productions:
    ///     flow_mapping_entry   ::= flow_node | KEY flow_node? (VALUE flow_node?)?
    ///     *                  ***** *
    /// </summary>
    Event ParseFlowMappingValue(bool isEmpty) {
        if (isEmpty) {
            state = ParserState.YamlParseFlowMappingKeyState;
            return ProcessEmptyScalar(GetCurrentToken().Start);
        }

        if (GetCurrentToken() is Value) {
            Skip();
            if (!(GetCurrentToken() is FlowEntry || GetCurrentToken() is FlowMappingEnd)) {
                states.Push(ParserState.YamlParseFlowMappingKeyState);
                return ParseNode(false, false);
            }
        }

        state = ParserState.YamlParseFlowMappingKeyState;
        return ProcessEmptyScalar(GetCurrentToken().Start);
    }
}
