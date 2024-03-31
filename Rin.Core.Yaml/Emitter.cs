using Rin.Core.Yaml.Events;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using TagDirective = Rin.Core.Yaml.Tokens.TagDirective;
using VersionDirective = Rin.Core.Yaml.Tokens.VersionDirective;

namespace Rin.Core.Yaml;

/// <summary>
///     Emits YAML streams.
/// </summary>
public class Emitter : IEmitter {
    internal const int MinBestIndent = 2;
    internal const int MaxBestIndent = 9;

    const int MaxAliasLength = 128;
    readonly TextWriter output;

    readonly bool isCanonical;
    readonly int bestIndent;
    readonly int bestWidth;
    EmitterState state;

    readonly Stack<EmitterState> states = new();
    readonly Queue<ParsingEvent> events = new();
    readonly Stack<int> indents = new();
    readonly TagDirectiveCollection tagDirectives = [];
    int indent;
    int flowLevel;
    bool isMappingContext;
    bool isSimpleKeyContext;
    bool isRootContext;

    int line;
    int column;
    bool isWhitespace;
    bool isIndentation;

    bool isOpenEnded;
    AnchorData anchorData;
    TagData tagData;
    ScalarData scalarData;

    static readonly Regex uriReplacer = new(@"[^0-9A-Za-z_\-;?@=$~\\\)\]/:&+,\.\*\(\[!]", RegexOptions.Singleline);

    /// <summary>
    ///     Gets or sets a value indicating whether [always indent].
    /// </summary>
    /// <value><c>true</c> if [always indent]; otherwise, <c>false</c>.</value>
    public bool ForceIndentLess { get; set; }

    bool IsUnicode =>
        output.Encoding == Encoding.UTF8
        || output.Encoding == Encoding.Unicode
        || output.Encoding == Encoding.BigEndianUnicode;

    /// <summary>
    ///     Initializes a new instance of the <see cref="IEmitter" /> class.
    /// </summary>
    /// <param name="output">The <see cref="TextWriter" /> where the emitter will write.</param>
    /// <param name="bestIndent">The preferred indentation.</param>
    /// <param name="bestWidth">The preferred text width.</param>
    /// <param name="isCanonical">If true, write the output in canonical form.</param>
    /// <param name="forceIndentLess">if set to <c>true</c> [always indent].</param>
    /// <exception cref="System.ArgumentOutOfRangeException">
    ///     bestIndent
    ///     or
    ///     bestWidth;The bestWidth parameter must be greater than bestIndent * 2.
    /// </exception>
    public Emitter(
        TextWriter output,
        int bestIndent = MinBestIndent,
        int bestWidth = int.MaxValue,
        bool isCanonical = false,
        bool forceIndentLess = false
    ) {
        if (bestIndent is < MinBestIndent or > MaxBestIndent) {
            throw new ArgumentOutOfRangeException(
                nameof(bestIndent),
                string.Format(
                    CultureInfo.InvariantCulture,
                    "The bestIndent parameter must be between {0} and {1}.",
                    MinBestIndent,
                    MaxBestIndent
                )
            );
        }

        this.bestIndent = bestIndent;
        if (bestWidth <= bestIndent * 2) {
            throw new ArgumentOutOfRangeException(
                nameof(bestWidth),
                "The bestWidth parameter must be greater than bestIndent * 2."
            );
        }

        this.bestWidth = bestWidth;
        this.isCanonical = isCanonical;
        ForceIndentLess = forceIndentLess;
        this.output = output;
    }

    /// <summary>
    ///     Emit an evt.
    /// </summary>
    public void Emit(ParsingEvent @event) {
        events.Enqueue(@event);

        while (!NeedMoreEvents()) {
            var current = events.Peek();
            AnalyzeEvent(current);
            StateMachine(current);

            // Only dequeue after calling state_machine because it checks how many events are in the queue.
            events.Dequeue();
        }
    }

    void Write(char value) {
        output.Write(value);
        ++column;
    }

    void Write(string value) {
        output.Write(value);
        column += value.Length;
    }

    void WriteBreak() {
        output.WriteLine();
        column = 0;
        ++line;
    }

    /// <summary>
    ///     Check if we need to accumulate more events before emitting.
    ///     We accumulate extra
    ///     - 1 event for DOCUMENT-START
    ///     - 2 events for SEQUENCE-START
    ///     - 3 events for MAPPING-START
    /// </summary>
    bool NeedMoreEvents() {
        if (events.Count == 0) {
            return true;
        }

        int accumulate;
        switch (events.Peek().Type) {
            case EventType.YamlDocumentStartEvent:
                accumulate = 1;
                break;

            case EventType.YamlSequenceStartEvent:
                accumulate = 2;
                break;

            case EventType.YamlMappingStartEvent:
                accumulate = 3;
                break;

            default:
                return false;
        }

        if (events.Count > accumulate) {
            return false;
        }

        var level = 0;
        foreach (var evt in events) {
            switch (evt.Type) {
                case EventType.YamlDocumentStartEvent:
                case EventType.YamlSequenceStartEvent:
                case EventType.YamlMappingStartEvent:
                    ++level;
                    break;

                case EventType.YamlDocumentEndEvent:
                case EventType.YamlSequenceEndEvent:
                case EventType.YamlMappingEndEvent:
                    --level;
                    break;
            }

            if (level == 0) {
                return false;
            }
        }

        return true;
    }

    void AnalyzeAnchor(string anchor, bool isAlias) {
        anchorData.anchor = anchor;
        anchorData.isAlias = isAlias;
    }

    /// <summary>
    ///     Check if the evt data is valid.
    /// </summary>
    void AnalyzeEvent(ParsingEvent evt) {
        anchorData.anchor = null;
        tagData.handle = null;
        tagData.suffix = null;

        if (evt is AnchorAlias alias) {
            AnalyzeAnchor(alias.Value, true);
            return;
        }

        if (evt is NodeEvent nodeEvent) {
            if (evt is Scalar scalar) {
                AnalyzeScalar(scalar.Value);
            }

            AnalyzeAnchor(nodeEvent.Anchor, false);
            if (!string.IsNullOrEmpty(nodeEvent.Tag) && (isCanonical || nodeEvent.IsCanonical)) {
                AnalyzeTag(nodeEvent.Tag);
            }
        }
    }

    /// <summary>
    ///     Check if a scalar is valid.
    /// </summary>
    void AnalyzeScalar(string value) {
        var blockIndicators = false;
        var flowIndicators = false;
        var lineBreaks = false;
        var specialCharacters = false;
        var tabs = false;

        var leadingSpace = false;
        var leadingBreak = false;
        var trailingSpace = false;
        var trailingBreak = false;
        var breakSpace = false;
        var spaceBreak = false;

        var previousSpace = false;
        var previousBreak = false;

        scalarData.value = value;

        if (value.Length == 0) {
            scalarData.isMultiline = false;
            scalarData.isFlowPlainAllowed = false;
            scalarData.isBlockPlainAllowed = true;
            scalarData.isSingleQuotedAllowed = true;
            scalarData.isFoldAllowed = false;
            return;
        }

        if (value.StartsWith("---", StringComparison.Ordinal) || value.StartsWith("...", StringComparison.Ordinal)) {
            blockIndicators = true;
            flowIndicators = true;
        }

        var precededByWhitespace = true;

        var buffer = new CharacterAnalyzer<StringLookAheadBuffer>(new(value));
        var followedByWhitespace = buffer.IsBlankOrBreakOrZero(1);

        var isFirst = true;
        while (!buffer.EndOfInput) {
            if (isFirst) {
                if (buffer.Check(@"#,[]{}&*!|>\""%@`")) {
                    flowIndicators = true;
                    blockIndicators = true;
                }

                if (buffer.Check("?:")) {
                    flowIndicators = true;
                    if (followedByWhitespace) {
                        blockIndicators = true;
                    }
                }

                if (buffer.Check('-') && followedByWhitespace) {
                    flowIndicators = true;
                    blockIndicators = true;
                }
            } else {
                if (buffer.Check(",?[]{}")) {
                    flowIndicators = true;
                }

                if (buffer.Check(':')) {
                    flowIndicators = true;
                    if (followedByWhitespace) {
                        blockIndicators = true;
                    }
                }

                if (buffer.Check('#') && precededByWhitespace) {
                    flowIndicators = true;
                    blockIndicators = true;
                }
            }

            if (!buffer.IsPrintable() || (!buffer.IsAscii() && !IsUnicode)) {
                specialCharacters = true;
            }

            if (buffer.IsTab()) {
                tabs = true;
            }

            if (buffer.IsBreak()) {
                lineBreaks = true;
            }

            if (buffer.IsSpace()) {
                if (isFirst) {
                    leadingSpace = true;
                }

                if (buffer.Buffer.Position >= buffer.Buffer.Length - 1) {
                    trailingSpace = true;
                }

                if (previousBreak) {
                    breakSpace = true;
                }

                previousSpace = true;
                previousBreak = false;
            } else if (buffer.IsBreak()) {
                if (isFirst) {
                    leadingBreak = true;
                }

                if (buffer.Buffer.Position >= buffer.Buffer.Length - 1) {
                    trailingBreak = true;
                }

                if (previousSpace) {
                    spaceBreak = true;
                }

                previousSpace = false;
                previousBreak = true;
            } else {
                previousSpace = false;
                previousBreak = false;
            }

            precededByWhitespace = buffer.IsBlankOrBreakOrZero();
            buffer.Skip(1);
            if (!buffer.EndOfInput) {
                followedByWhitespace = buffer.IsBlankOrBreakOrZero(1);
            }

            isFirst = false;
        }

        scalarData.isMultiline = lineBreaks;
        scalarData.isFlowPlainAllowed = true;
        scalarData.isBlockPlainAllowed = true;
        scalarData.isSingleQuotedAllowed = true;
        scalarData.isFoldAllowed = true;
        scalarData.isLiteralAllowed = true;

        if (leadingSpace || leadingBreak || trailingSpace || trailingBreak) {
            scalarData.isFlowPlainAllowed = false;
            scalarData.isBlockPlainAllowed = false;
        }

        if (trailingSpace) {
            scalarData.isFoldAllowed = false;
        }

        if (breakSpace) {
            scalarData.isFlowPlainAllowed = false;
            scalarData.isBlockPlainAllowed = false;
            scalarData.isSingleQuotedAllowed = false;
        }

        if (spaceBreak || specialCharacters || tabs) {
            scalarData.isFlowPlainAllowed = false;
            scalarData.isBlockPlainAllowed = false;
            scalarData.isSingleQuotedAllowed = false;
            scalarData.isFoldAllowed = false;
        }

        if (specialCharacters) {
            scalarData.isLiteralAllowed = false;
        }

        if (lineBreaks) {
            scalarData.isFlowPlainAllowed = false;
            scalarData.isBlockPlainAllowed = false;
        }

        if (flowIndicators) {
            scalarData.isFlowPlainAllowed = false;
        }

        if (blockIndicators) {
            scalarData.isBlockPlainAllowed = false;
        }
    }

    /// <summary>
    ///     Check if a tag is valid.
    /// </summary>
    void AnalyzeTag(string tag) {
        tagData.handle = tag;
        foreach (var tagDirective in tagDirectives) {
            if (tag.StartsWith(tagDirective.Prefix, StringComparison.Ordinal)) {
                tagData.handle = tagDirective.Handle;
                tagData.suffix = tag.Substring(tagDirective.Prefix.Length);
                break;
            }
        }
    }

    /// <summary>
    ///     State dispatcher.
    /// </summary>
    void StateMachine(ParsingEvent evt) {
        switch (state) {
            case EmitterState.YamlEmitStreamStartState:
                EmitStreamStart(evt);
                break;

            case EmitterState.YamlEmitFirstDocumentStartState:
                EmitDocumentStart(evt, true);
                break;

            case EmitterState.YamlEmitDocumentStartState:
                EmitDocumentStart(evt, false);
                break;

            case EmitterState.YamlEmitDocumentContentState:
                EmitDocumentContent(evt);
                break;

            case EmitterState.YamlEmitDocumentEndState:
                EmitDocumentEnd(evt);
                break;

            case EmitterState.YamlEmitFlowSequenceFirstItemState:
                EmitFlowSequenceItem(evt, true);
                break;

            case EmitterState.YamlEmitFlowSequenceItemState:
                EmitFlowSequenceItem(evt, false);
                break;

            case EmitterState.YamlEmitFlowMappingFirstKeyState:
                EmitFlowMappingKey(evt, true);
                break;

            case EmitterState.YamlEmitFlowMappingKeyState:
                EmitFlowMappingKey(evt, false);
                break;

            case EmitterState.YamlEmitFlowMappingSimpleValueState:
                EmitFlowMappingValue(evt, true);
                break;

            case EmitterState.YamlEmitFlowMappingValueState:
                EmitFlowMappingValue(evt, false);
                break;

            case EmitterState.YamlEmitBlockSequenceFirstItemState:
                EmitBlockSequenceItem(evt, true);
                break;

            case EmitterState.YamlEmitBlockSequenceItemState:
                EmitBlockSequenceItem(evt, false);
                break;

            case EmitterState.YamlEmitBlockMappingFirstKeyState:
                EmitBlockMappingKey(evt, true);
                break;

            case EmitterState.YamlEmitBlockMappingKeyState:
                EmitBlockMappingKey(evt, false);
                break;

            case EmitterState.YamlEmitBlockMappingSimpleValueState:
                EmitBlockMappingValue(evt, true);
                break;

            case EmitterState.YamlEmitBlockMappingValueState:
                EmitBlockMappingValue(evt, false);
                break;

            case EmitterState.YamlEmitEndState:
                throw new YamlException("Expected nothing after STREAM-END");

            default:
                Debug.Assert(false, "Invalid state.");
                throw new InvalidOperationException("Invalid state");
        }
    }

    /// <summary>
    ///     Expect STREAM-START.
    /// </summary>
    void EmitStreamStart(ParsingEvent evt) {
        if (evt is not StreamStart) {
            throw new ArgumentException("Expected STREAM-START.", nameof(evt));
        }

        indent = -1;
        line = 0;
        column = 0;
        isWhitespace = true;
        isIndentation = true;
        state = EmitterState.YamlEmitFirstDocumentStartState;
    }

    /// <summary>
    ///     Expect DOCUMENT-START or STREAM-END.
    /// </summary>
    void EmitDocumentStart(ParsingEvent evt, bool isFirst) {
        if (evt is DocumentStart documentStart) {
            var isImplicit = documentStart.IsImplicit && isFirst && !isCanonical;

            if (documentStart.Version != null && isOpenEnded) {
                WriteIndicator("...", true, false, false);
                WriteIndent();
            }

            if (documentStart.Version != null) {
                AnalyzeVersionDirective(documentStart.Version);

                isImplicit = false;
                WriteIndicator("%YAML", true, false, false);
                WriteIndicator(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "{0}.{1}",
                        Constants.MajorVersion,
                        Constants.MinorVersion
                    ),
                    true,
                    false,
                    false
                );
                WriteIndent();
            }

            if (documentStart.Tags != null) {
                foreach (var tagDirective in documentStart.Tags) {
                    AppendTagDirective(tagDirective, false);
                }
            }

            foreach (var tagDirective in Constants.DefaultTagDirectives) {
                AppendTagDirective(tagDirective, true);
            }

            if (documentStart.Tags != null
                && documentStart.Tags.Count != 0
                && !documentStart.Tags.SequenceEqual(Constants.DefaultTagDirectives)) {
                isImplicit = false;
                foreach (var tagDirective in documentStart.Tags) {
                    WriteIndicator("%TAG", true, false, false);
                    WriteTagHandle(tagDirective.Handle);
                    WriteTagContent(tagDirective.Prefix, true);
                    WriteIndent();
                }
            }

            if (CheckEmptyDocument()) {
                isImplicit = false;
            }

            if (!isImplicit) {
                WriteIndent();
                WriteIndicator("---", true, false, false);
                if (isCanonical) {
                    WriteIndent();
                }
            }

            state = EmitterState.YamlEmitDocumentContentState;
        } else if (evt is StreamEnd) {
            if (isOpenEnded) {
                WriteIndicator("...", true, false, false);
                WriteIndent();
            }

            state = EmitterState.YamlEmitEndState;
        } else {
            throw new YamlException("Expected DOCUMENT-START or STREAM-END");
        }
    }

    /// <summary>
    ///     Check if the document content is an empty scalar.
    /// </summary>
    bool CheckEmptyDocument() {
        var index = 0;
        foreach (var parsingEvent in events) {
            if (++index == 2) {
                if (parsingEvent is Scalar scalar) {
                    return string.IsNullOrEmpty(scalar.Value);
                }

                break;
            }
        }

        return false;
    }

    void WriteTagHandle(string value) {
        if (!isWhitespace) {
            Write(' ');
        }

        Write(value);

        isWhitespace = false;
        isIndentation = false;
    }

    static string UrlEncode(string text) {
        return uriReplacer.Replace(
            text,
            delegate(Match match) {
                var buffer = new StringBuilder();
                foreach (var toEncode in Encoding.UTF8.GetBytes(match.Value)) {
                    buffer.Append($"%{toEncode:X02}");
                }

                return buffer.ToString();
            }
        );
    }

    void WriteTagContent(string value, bool needsWhitespace) {
        if (needsWhitespace && !isWhitespace) {
            Write(' ');
        }

        Write(UrlEncode(value));

        isWhitespace = false;
        isIndentation = false;
    }

    /// <summary>
    ///     Append a directive to the directives stack.
    /// </summary>
    void AppendTagDirective(TagDirective value, bool allowDuplicates) {
        if (tagDirectives.Contains(value)) {
            if (allowDuplicates) {
                return;
            }

            throw new YamlException("Duplicate %TAG directive.");
        }

        tagDirectives.Add(value);
    }

    /// <summary>
    ///     Check if a %YAML directive is valid.
    /// </summary>
    static void AnalyzeVersionDirective(VersionDirective versionDirective) {
        if (versionDirective.Version.Major != Constants.MajorVersion
            || versionDirective.Version.Minor != Constants.MinorVersion) {
            throw new YamlException("Incompatible %YAML directive");
        }
    }

    void WriteIndicator(string indicator, bool needWhitespace, bool whitespace, bool indentation) {
        if (needWhitespace && !isWhitespace) {
            Write(' ');
        }

        Write(indicator);

        isWhitespace = whitespace;
        isIndentation &= indentation;
        isOpenEnded = false;
    }

    void WriteIndent() {
        var currentIndent = Math.Max(indent, 0);

        if (!isIndentation || column > currentIndent || (column == currentIndent && !isWhitespace)) {
            WriteBreak();
        }

        while (column < currentIndent) {
            Write(' ');
        }

        isWhitespace = true;
        isIndentation = true;
    }

    /// <summary>
    ///     Expect the root node.
    /// </summary>
    void EmitDocumentContent(ParsingEvent evt) {
        states.Push(EmitterState.YamlEmitDocumentEndState);
        EmitNode(evt, true, false, false);
    }

    /// <summary>
    ///     Expect a node.
    /// </summary>
    void EmitNode(ParsingEvent evt, bool isRoot, bool isMapping, bool isSimpleKey) {
        isRootContext = isRoot;
        isMappingContext = isMapping;
        isSimpleKeyContext = isSimpleKey;

        var eventType = evt.Type;
        switch (eventType) {
            case EventType.YamlAliasEvent:
                EmitAlias();
                break;

            case EventType.YamlScalarEvent:
                EmitScalar(evt);
                break;

            case EventType.YamlSequenceStartEvent:
                EmitSequenceStart(evt);
                break;

            case EventType.YamlMappingStartEvent:
                EmitMappingStart(evt);
                break;

            default:
                throw new YamlException($"Expected SCALAR, SEQUENCE-START, MAPPING-START, or ALIAS, got {eventType}");
        }
    }

    /// <summary>
    ///     Expect SEQUENCE-START.
    /// </summary>
    void EmitSequenceStart(ParsingEvent evt) {
        ProcessAnchor();
        ProcessTag();

        var sequenceStart = (SequenceStart)evt;

        if (flowLevel != 0 || isCanonical || sequenceStart.Style == DataStyle.Compact || CheckEmptySequence()) {
            state = EmitterState.YamlEmitFlowSequenceFirstItemState;
        } else {
            state = EmitterState.YamlEmitBlockSequenceFirstItemState;
        }
    }

    /// <summary>
    ///     Check if the next events represent an empty sequence.
    /// </summary>
    bool CheckEmptySequence() {
        if (events.Count < 2) {
            return false;
        }

        var eventList = new FakeList<ParsingEvent>(events);
        return eventList[0] is SequenceStart && eventList[1] is SequenceEnd;
    }

    /// <summary>
    ///     Check if the next events represent an empty mapping.
    /// </summary>
    bool CheckEmptyMapping() {
        if (events.Count < 2) {
            return false;
        }

        var eventList = new FakeList<ParsingEvent>(events);
        return eventList[0] is MappingStart && eventList[1] is MappingEnd;
    }

    /// <summary>
    ///     Write a tag.
    /// </summary>
    void ProcessTag() {
        if (tagData.handle == null && tagData.suffix == null) {
            return;
        }

        if (tagData.handle != null) {
            WriteTagHandle(tagData.handle);
            if (tagData.suffix != null) {
                WriteTagContent(tagData.suffix, false);
            }
        } else {
            WriteIndicator("!<", true, false, false);
            WriteTagContent(tagData.suffix, false);
            WriteIndicator(">", false, false, false);
        }
    }

    /// <summary>
    ///     Expect MAPPING-START.
    /// </summary>
    void EmitMappingStart(ParsingEvent evt) {
        ProcessAnchor();
        ProcessTag();

        var mappingStart = (MappingStart)evt;

        if (flowLevel != 0 || isCanonical || mappingStart.Style == DataStyle.Compact || CheckEmptyMapping()) {
            state = EmitterState.YamlEmitFlowMappingFirstKeyState;
        } else {
            state = EmitterState.YamlEmitBlockMappingFirstKeyState;
        }
    }

    /// <summary>
    ///     Expect SCALAR.
    /// </summary>
    void EmitScalar(ParsingEvent evt) {
        SelectScalarStyle(evt);
        ProcessAnchor();
        ProcessTag();
        IncreaseIndent(true, false);
        ProcessScalar();

        indent = indents.Pop();
        state = states.Pop();
    }

    /// <summary>
    ///     Write a scalar.
    /// </summary>
    void ProcessScalar() {
        switch (scalarData.style) {
            case ScalarStyle.Plain:
                WritePlainScalar(scalarData.value, !isSimpleKeyContext);
                break;

            case ScalarStyle.SingleQuoted:
                WriteSingleQuotedScalar(scalarData.value, !isSimpleKeyContext);
                break;

            case ScalarStyle.DoubleQuoted:
                WriteDoubleQuotedScalar(scalarData.value, !isSimpleKeyContext);
                break;

            case ScalarStyle.Literal:
                WriteLiteralScalar(scalarData.value);
                break;

            case ScalarStyle.Folded:
                WriteFoldedScalar(scalarData.value);
                break;

            default:
                // Impossible.
                throw new InvalidOperationException();
        }
    }

    static bool IsBreak(char character) => character is '\r' or '\n' or '\x85' or '\x2028' or '\x2029';

    static bool IsBlank(char character) => character is ' ' or '\t';

    /// <summary>
    ///     Check if the specified character is a space.
    /// </summary>
    static bool IsSpace(char character) => character == ' ';

    void WriteFoldedScalar(string value) {
        var previousBreak = true;
        var leadingSpaces = true;

        WriteIndicator(">", true, false, false);
        WriteBlockScalarHints(value);
        WriteBreak();

        isIndentation = true;
        isWhitespace = true;

        for (var i = 0; i < value.Length; ++i) {
            var character = value[i];

            // Treat CRLF as a single line break
            if (character == '\r' && i + 1 < value.Length && value[i + 1] == '\n') {
                continue;
            }

            if (IsBreak(character)) {
                if (!previousBreak && !leadingSpaces && character == '\n') {
                    var k = 0;
                    while (i + k < value.Length && IsBreak(value[i + k])) {
                        ++k;
                    }

                    if (i + k < value.Length && !(IsBlank(value[i + k]) || IsBreak(value[i + k]))) {
                        WriteBreak();
                    }
                }

                WriteBreak();
                isIndentation = true;
                previousBreak = true;
            } else {
                if (previousBreak) {
                    WriteIndent();
                    leadingSpaces = IsBlank(character);
                }

                if (
                    !previousBreak
                    && character == ' '
                    && i + 1 < value.Length
                    && value[i + 1] != ' '
                    && column > bestWidth
                ) {
                    WriteIndent();
                } else {
                    Write(character);
                }

                isIndentation = false;
                previousBreak = false;
            }
        }
    }

    void WriteLiteralScalar(string value) {
        var previousBreak = true;

        WriteIndicator("|", true, false, false);
        WriteBlockScalarHints(value);
        WriteBreak();

        isIndentation = true;
        isWhitespace = true;

        for (var i = 0; i < value.Length; i++) {
            var character = value[i];

            // Treat CRLF as a single line break
            if (character == '\r' && i + 1 < value.Length && value[i + 1] == '\n') {
                continue;
            }

            if (IsBreak(character)) {
                WriteBreak();
                isIndentation = true;
                previousBreak = true;
            } else {
                if (previousBreak) {
                    WriteIndent();
                }

                Write(character);
                isIndentation = false;
                previousBreak = false;
            }
        }
    }

    void WriteDoubleQuotedScalar(string value, bool allowBreaks) {
        WriteIndicator("\"", true, false, false);

        var previousSpace = false;
        for (var index = 0; index < value.Length; ++index) {
            var character = value[index];


            if (
                !IsPrintable(character)
                || IsBreak(character)
                || character == '\x9'
                || character == '"'
                || character == '\\'
            ) {
                Write('\\');

                switch (character) {
                    case '\0':
                        Write('0');
                        break;

                    case '\x7':
                        Write('a');
                        break;

                    case '\x8':
                        Write('b');
                        break;

                    case '\x9':
                        Write('t');
                        break;

                    case '\xA':
                        Write('n');
                        break;

                    case '\xB':
                        Write('v');
                        break;

                    case '\xC':
                        Write('f');
                        break;

                    case '\xD':
                        Write('r');
                        break;

                    case '\x1B':
                        Write('e');
                        break;

                    case '\x22':
                        Write('"');
                        break;

                    case '\x5C':
                        Write('\\');
                        break;

                    case '\x85':
                        Write('N');
                        break;

                    case '\xA0':
                        Write('_');
                        break;

                    case '\x2028':
                        Write('L');
                        break;

                    case '\x2029':
                        Write('P');
                        break;

                    default:
                        var code = (short)character;
                        if (code <= 0xFF) {
                            Write('x');
                            Write(code.ToString("X02", CultureInfo.InvariantCulture));
                        } else {
                            //if (code <= 0xFFFF) {
                            Write('u');
                            Write(code.ToString("X04", CultureInfo.InvariantCulture));
                        }

                        //else {
                        //	Write('U');
                        //	Write(code.ToString("X08"));
                        //}
                        break;
                }

                previousSpace = false;
            } else if (character == ' ') {
                if (allowBreaks && !previousSpace && column > bestWidth && index > 0 && index + 1 < value.Length) {
                    WriteIndent();
                    if (value[index + 1] == ' ') {
                        Write('\\');
                    }
                } else {
                    Write(character);
                }

                previousSpace = true;
            } else {
                Write(character);
                previousSpace = false;
            }
        }

        WriteIndicator("\"", false, false, false);

        isWhitespace = false;
        isIndentation = false;
    }

    void WriteSingleQuotedScalar(string value, bool allowBreaks) {
        WriteIndicator("'", true, false, false);

        var previousSpace = false;
        var previousBreak = false;

        for (var index = 0; index < value.Length; ++index) {
            var character = value[index];

            if (character == ' ') {
                if (
                    allowBreaks
                    && !previousSpace
                    && column > bestWidth
                    && index != 0
                    && index + 1 < value.Length
                    && value[index + 1] != ' '
                ) {
                    WriteIndent();
                } else {
                    Write(character);
                }

                previousSpace = true;
            } else if (IsBreak(character)) {
                if (!previousBreak && character == '\n') {
                    WriteBreak();
                }

                WriteBreak();
                isIndentation = true;
                previousBreak = true;
            } else {
                if (previousBreak) {
                    WriteIndent();
                }

                if (character == '\'') {
                    Write(character);
                }

                Write(character);
                isIndentation = false;
                previousSpace = false;
                previousBreak = false;
            }
        }

        WriteIndicator("'", false, false, false);

        isWhitespace = false;
        isIndentation = false;
    }

    void WritePlainScalar(string value, bool allowBreaks) {
        if (!isWhitespace) {
            Write(' ');
        }

        var previousSpace = false;
        var previousBreak = false;
        for (var index = 0; index < value.Length; ++index) {
            var character = value[index];

            if (IsSpace(character)) {
                if (allowBreaks
                    && !previousSpace
                    && column > bestWidth
                    && index + 1 < value.Length
                    && value[index + 1] != ' ') {
                    WriteIndent();
                } else {
                    Write(character);
                }

                previousSpace = true;
            } else if (IsBreak(character)) {
                if (!previousBreak && character == '\n') {
                    WriteBreak();
                }

                WriteBreak();
                isIndentation = true;
                previousBreak = true;
            } else {
                if (previousBreak) {
                    WriteIndent();
                }

                Write(character);
                isIndentation = false;
                previousSpace = false;
                previousBreak = false;
            }
        }

        isWhitespace = false;
        isIndentation = false;

        if (isRootContext) {
            isOpenEnded = true;
        }
    }

    /// <summary>
    ///     Increase the indentation level.
    /// </summary>
    void IncreaseIndent(bool isFlow, bool isIndentless) {
        indents.Push(indent);

        if (indent < 0) {
            indent = isFlow ? bestIndent : 0;
        } else if (!isIndentless || !ForceIndentLess) {
            indent += bestIndent;
        }
    }

    /// <summary>
    ///     Determine an acceptable scalar style.
    /// </summary>
    void SelectScalarStyle(ParsingEvent evt) {
        var scalar = (Scalar)evt;

        var style = scalar.Style;
        var noTag = tagData.handle == null && tagData.suffix == null;

        if (noTag && scalar is { IsPlainImplicit: false, IsQuotedImplicit: false }) {
            throw new YamlException("Neither tag nor isImplicit flags are specified.");
        }

        if (style == ScalarStyle.Any) {
            style = scalarData.isMultiline ? ScalarStyle.Literal : ScalarStyle.Plain;
        }

        if (isCanonical) {
            style = ScalarStyle.DoubleQuoted;
        }

        // Note: if length is 1, it might be a single char and going literal might transform \n into \r\n (which is no good)
        if ((isSimpleKeyContext || scalar.Value.Length <= 1) && scalarData.isMultiline) {
            style = ScalarStyle.DoubleQuoted;
        }

        if (style == ScalarStyle.Plain) {
            if ((flowLevel != 0 && !scalarData.isFlowPlainAllowed)
                || (flowLevel == 0 && !scalarData.isBlockPlainAllowed)) {
                style = ScalarStyle.SingleQuoted;
            }

            if (string.IsNullOrEmpty(scalarData.value) && (flowLevel != 0 || isSimpleKeyContext)) {
                style = ScalarStyle.SingleQuoted;
            }

            if (noTag && !scalar.IsPlainImplicit) {
                style = ScalarStyle.SingleQuoted;
            }
        }

        if (style == ScalarStyle.SingleQuoted) {
            if (!scalarData.isSingleQuotedAllowed) {
                style = ScalarStyle.DoubleQuoted;
            }
        }

        if (style == ScalarStyle.Literal || style == ScalarStyle.Folded) {
            if ((!scalarData.isFoldAllowed && style == ScalarStyle.Folded)
                || (!scalarData.isLiteralAllowed && style == ScalarStyle.Literal)
                || flowLevel != 0
                || isSimpleKeyContext) {
                style = ScalarStyle.DoubleQuoted;
            }
        }

        // TODO: What is this code supposed to mean?
        //if (noTag && !scalar.IsQuotedImplicit && style != ScalarStyle.Plain)
        //{
        //	tagData.handle = "!";
        //}

        scalarData.style = style;
    }

    /// <summary>
    ///     Expect ALIAS
    /// </summary>
    void EmitAlias() {
        ProcessAnchor();
        state = states.Pop();
    }

    /// <summary>
    ///     Write an anchor
    /// </summary>
    void ProcessAnchor() {
        if (anchorData.anchor != null) {
            WriteIndicator(anchorData.isAlias ? "*" : "&", true, false, false);
            WriteAnchor(anchorData.anchor);
        }
    }

    void WriteAnchor(string value) {
        Write(value);

        isWhitespace = false;
        isIndentation = false;
    }

    /// <summary>
    ///     Expect DOCUMENT-END.
    /// </summary>
    void EmitDocumentEnd(ParsingEvent evt) {
        if (evt is DocumentEnd documentEnd) {
            WriteIndent();
            if (!documentEnd.IsImplicit) {
                WriteIndicator("...", true, false, false);
                WriteIndent();
            }

            state = EmitterState.YamlEmitDocumentStartState;
            tagDirectives.Clear();
        } else {
            throw new YamlException("Expected DOCUMENT-END.");
        }
    }

    /// <summary>
    ///     Expect a flow item node.
    /// </summary>
    void EmitFlowSequenceItem(ParsingEvent evt, bool isFirst) {
        if (isFirst) {
            WriteIndicator("[", true, true, false);
            IncreaseIndent(true, false);
            ++flowLevel;
        }

        if (evt is SequenceEnd) {
            --flowLevel;
            indent = indents.Pop();
            if (isCanonical && !isFirst) {
                WriteIndicator(",", false, false, false);
                WriteIndent();
            }

            WriteIndicator("]", false, false, false);
            state = states.Pop();
            return;
        }

        if (!isFirst) {
            WriteIndicator(",", false, false, false);
        }

        if (isCanonical || column > bestWidth) {
            WriteIndent();
        }

        states.Push(EmitterState.YamlEmitFlowSequenceItemState);
        EmitNode(evt, false, false, false);
    }

    /// <summary>
    ///     Expect a flow key node.
    /// </summary>
    void EmitFlowMappingKey(ParsingEvent evt, bool isFirst) {
        if (isFirst) {
            WriteIndicator("{", true, true, false);
            IncreaseIndent(true, false);
            ++flowLevel;
        }

        if (evt is MappingEnd) {
            --flowLevel;
            indent = indents.Pop();
            if (isCanonical && !isFirst) {
                WriteIndicator(",", false, false, false);
                WriteIndent();
            }

            WriteIndicator("}", false, false, false);
            state = states.Pop();
            return;
        }

        if (!isFirst) {
            WriteIndicator(",", false, false, false);
        }

        if (isCanonical || column > bestWidth) {
            WriteIndent();
        }

        if (!isCanonical && CheckSimpleKey()) {
            states.Push(EmitterState.YamlEmitFlowMappingSimpleValueState);
            EmitNode(evt, false, true, true);
        } else {
            WriteIndicator("?", true, false, false);
            states.Push(EmitterState.YamlEmitFlowMappingValueState);
            EmitNode(evt, false, true, false);
        }
    }

    static int SafeStringLength(string value) => value != null ? value.Length : 0;

    /// <summary>
    ///     Check if the next node can be expressed as a simple key.
    /// </summary>
    bool CheckSimpleKey() {
        if (events.Count < 1) {
            return false;
        }

        int length;
        switch (events.Peek().Type) {
            case EventType.YamlAliasEvent:
                length = SafeStringLength(anchorData.anchor);
                break;

            case EventType.YamlScalarEvent:
                if (scalarData.isMultiline) {
                    return false;
                }

                length =
                    SafeStringLength(anchorData.anchor)
                    + SafeStringLength(tagData.handle)
                    + SafeStringLength(tagData.suffix)
                    + SafeStringLength(scalarData.value);
                break;

            case EventType.YamlSequenceStartEvent:
                if (!CheckEmptySequence()) {
                    return false;
                }

                length =
                    SafeStringLength(anchorData.anchor)
                    + SafeStringLength(tagData.handle)
                    + SafeStringLength(tagData.suffix);
                break;

            case EventType.YamlMappingStartEvent:
                if (!CheckEmptySequence()) {
                    return false;
                }

                length =
                    SafeStringLength(anchorData.anchor)
                    + SafeStringLength(tagData.handle)
                    + SafeStringLength(tagData.suffix);
                break;

            default:
                return false;
        }

        return length <= MaxAliasLength;
    }

    /// <summary>
    ///     Expect a flow value node.
    /// </summary>
    void EmitFlowMappingValue(ParsingEvent evt, bool isSimple) {
        if (isSimple) {
            WriteIndicator(":", false, false, false);
        } else {
            if (isCanonical || column > bestWidth) {
                WriteIndent();
            }

            WriteIndicator(":", true, false, false);
        }

        states.Push(EmitterState.YamlEmitFlowMappingKeyState);
        EmitNode(evt, false, true, false);
    }

    /// <summary>
    ///     Expect a block item node.
    /// </summary>
    void EmitBlockSequenceItem(ParsingEvent evt, bool isFirst) {
        if (isFirst) {
            IncreaseIndent(false, isMappingContext && !isIndentation);
        }

        if (evt is SequenceEnd) {
            indent = indents.Pop();
            state = states.Pop();
            return;
        }

        WriteIndent();
        WriteIndicator("-", true, false, true);
        states.Push(EmitterState.YamlEmitBlockSequenceItemState);

        EmitNode(evt, false, false, false);
    }

    /// <summary>
    ///     Expect a block key node.
    /// </summary>
    void EmitBlockMappingKey(ParsingEvent evt, bool isFirst) {
        if (isFirst) {
            IncreaseIndent(false, false);
        }

        if (evt is MappingEnd) {
            indent = indents.Pop();
            state = states.Pop();
            return;
        }

        WriteIndent();

        if (CheckSimpleKey()) {
            states.Push(EmitterState.YamlEmitBlockMappingSimpleValueState);
            EmitNode(evt, false, true, true);
        } else {
            WriteIndicator("?", true, false, true);
            states.Push(EmitterState.YamlEmitBlockMappingValueState);
            EmitNode(evt, false, true, false);
        }
    }

    /// <summary>
    ///     Expect a block value node.
    /// </summary>
    void EmitBlockMappingValue(ParsingEvent evt, bool isSimple) {
        if (isSimple) {
            WriteIndicator(":", false, false, false);
        } else {
            WriteIndent();
            WriteIndicator(":", true, false, true);
        }

        states.Push(EmitterState.YamlEmitBlockMappingKeyState);
        EmitNode(evt, false, true, false);
    }

    void WriteBlockScalarHints(string value) {
        var analyzer = new CharacterAnalyzer<StringLookAheadBuffer>(new(value));

        if (analyzer.IsBlank() || analyzer.IsBreak()) {
            var indentHint = string.Format(CultureInfo.InvariantCulture, "{0}", bestIndent);
            WriteIndicator(indentHint, false, false, false);
        }

        isOpenEnded = false;

        string? chompHint = null;
        if (value.Length == 0 || !analyzer.IsBreak(value.Length - 1)) {
            chompHint = "-";
        } else if ((value.Length >= 2 && analyzer.IsBreak(value.Length - 2))
                   || (value.Length == 1 && analyzer.IsBreak(0))) {
            chompHint = "+";
            isOpenEnded = true;
        }

        if (chompHint != null) {
            WriteIndicator(chompHint, false, false, false);
        }
    }

    internal static bool IsPrintable(char character) =>
        character is >= '\x20' and <= '\x7E'
            or '\x09'
            or '\x0A'
            or '\x0D'
            or '\x85'
            or >= '\xA0' and <= '\xD7FF'
            or >= '\xE000' and <= '\xFFFD';

    struct AnchorData {
        public string anchor;
        public bool isAlias;
    }

    struct TagData {
        public string handle;
        public string suffix;
    }

    struct ScalarData {
        public string value;
        public bool isMultiline;
        public bool isFlowPlainAllowed;
        public bool isBlockPlainAllowed;
        public bool isSingleQuotedAllowed;
        public bool isFoldAllowed;
        public bool isLiteralAllowed;
        public ScalarStyle style;
    }
}
