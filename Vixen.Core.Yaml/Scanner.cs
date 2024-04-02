using System.Diagnostics;
using System.Text;
using Vixen.Core.Yaml.Tokens;

namespace Vixen.Core.Yaml;

/// <summary>
///     Converts a sequence of characters into a sequence of YAML tokens.
/// </summary>
public class Scanner {
    const int MaxVersionNumberLength = 9;
    const int MaxBufferLength = 8;

    readonly Stack<int> indents = new();
    readonly InsertionQueue<Token> tokens = new();
    readonly Stack<SimpleKey> simpleKeys = new();

    // Used by ScanFlowScalar, this will reduce memory usage by /2
    readonly StringBuilder scanScalarValue = new();
    readonly StringBuilder scanScalarWhitespaces = new();
    readonly StringBuilder scanScalarLeadingBreak = new();
    readonly StringBuilder scanScalarTrailingBreaks = new();

    bool streamStartProduced;
    bool streamEndProduced;
    int indent = -1;
    bool simpleKeyAllowed;
    Mark mark;

    int flowLevel;
    int tokensParsed;
    readonly CharacterAnalyzer<LookAheadBuffer> analyzer;
    bool tokenAvailable;

    static readonly IDictionary<char, char> simpleEscapeCodes = InitializeSimpleEscapeCodes();

    /// <summary>
    ///     Gets the current position inside the input stream.
    /// </summary>
    /// <value>The current position.</value>
    public Mark CurrentPosition => mark;

    /// <summary>
    ///     Gets the current token.
    /// </summary>
    public Token? Current { get; private set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="Scanner" /> class.
    /// </summary>
    /// <param name="input">The input.</param>
    public Scanner(TextReader input) {
        analyzer = new(new(input, MaxBufferLength));
        mark.Column = 0;
        mark.Line = 0;
    }

    /// <summary>
    ///     Moves to the next token.
    /// </summary>
    /// <returns></returns>
    public bool MoveNext() {
        if (Current != null) {
            ConsumeCurrent();
        }

        return InternalMoveNext();
    }

    static IDictionary<char, char> InitializeSimpleEscapeCodes() {
        IDictionary<char, char> codes = new SortedDictionary<char, char>();
        codes.Add('0', '\0');
        codes.Add('a', '\x07');
        codes.Add('b', '\x08');
        codes.Add('t', '\x09');
        codes.Add('\t', '\x09');
        codes.Add('n', '\x0A');
        codes.Add('v', '\x0B');
        codes.Add('f', '\x0C');
        codes.Add('r', '\x0D');
        codes.Add('e', '\x1B');
        codes.Add(' ', '\x20');
        codes.Add('"', '"');
        codes.Add('\'', '\'');
        codes.Add('\\', '\\');
        codes.Add('N', '\x85');
        codes.Add('_', '\xA0');
        codes.Add('L', '\x2028');
        codes.Add('P', '\x2029');
        return codes;
    }

    char ReadCurrentCharacter() {
        var currentCharacter = analyzer.Peek(0);
        Skip();
        return currentCharacter;
    }

    void ReadLine(StringBuilder dest) {
        if (analyzer.Check("\r\n\x85")) {
            // CR LF -> LF  --- CR|LF|NEL -> LF
            SkipLine();
            dest.AppendLine();
        } else {
            var nextChar = analyzer.Peek(0); // LS|PS -> LS|PS
            SkipLine();
            dest.Append(nextChar);
        }
    }

    void FetchMoreTokens() {
        // While we need more tokens to fetch, do it.

        while (true) {
            // Check if we really need to fetch more tokens.
            var needsMoreTokens = false;
            if (tokens.Count == 0) {
                // Queue is empty.
                needsMoreTokens = true;
            } else {
                // Check if any potential simple key may occupy the head position.
                StaleSimpleKeys();

                if (simpleKeys.Any(simpleKey => simpleKey.IsPossible && simpleKey.TokenNumber == tokensParsed)) {
                    needsMoreTokens = true;
                }
            }

            // We are finished.
            if (!needsMoreTokens) {
                break;
            }

            // Fetch the next token.
            FetchNextToken();
        }

        tokenAvailable = true;
    }

    static bool StartsWith(StringBuilder what, char start) => what.Length > 0 && what[0] == start;

    /// <summary>
    ///     Check the list of potential simple keys and remove the positions that
    ///     cannot contain simple keys anymore.
    /// </summary>
    void StaleSimpleKeys() {
        // Check for a potential simple key for each flow level.

        foreach (var key in simpleKeys) {
            // The specification requires that a simple key
            //  - is limited to a single line,
            //  - is shorter than 1024 characters.

            if (key.IsPossible && (key.Mark.Line < mark.Line || key.Mark.Index + 1024 < mark.Index)) {
                // Check if the potential simple key to be removed is required.

                if (key.IsRequired) {
                    throw new SyntaxErrorException(
                        mark,
                        mark,
                        "While scanning a simple key, could not find expected ':'."
                    );
                }

                key.IsPossible = false;
            }
        }
    }

    void FetchNextToken() {
        // Check if we just started scanning.  Fetch STREAM-START then.
        if (!streamStartProduced) {
            FetchStreamStart();
            return;
        }

        // Eat whitespaces and comments until we reach the next token.
        ScanToNextToken();

        // Remove obsolete potential simple keys.
        StaleSimpleKeys();

        // Check the indentation level against the current column.
        UnrollIndent(mark.Column);

        // Ensure that the buffer contains at least 4 characters.  4 is the length
        // of the longest indicators ('--- ' and '... ').
        analyzer.Buffer.Cache(4);

        // Is it the end of the stream?
        if (analyzer.Buffer.EndOfInput) {
            FetchStreamEnd();
            return;
        }

        // Is it a directive?
        if (mark.Column == 0 && analyzer.Check('%')) {
            FetchDirective();
            return;
        }

        // Is it the document start indicator?
        var isDocumentStart =
            mark.Column == 0
            && analyzer.Check('-', 0)
            && analyzer.Check('-', 1)
            && analyzer.Check('-', 2)
            && analyzer.IsBlankOrBreakOrZero(3);

        if (isDocumentStart) {
            FetchDocumentIndicator(true);
            return;
        }

        // Is it the document end indicator?
        var isDocumentEnd =
            mark.Column == 0
            && analyzer.Check('.', 0)
            && analyzer.Check('.', 1)
            && analyzer.Check('.', 2)
            && analyzer.IsBlankOrBreakOrZero(3);

        if (isDocumentEnd) {
            FetchDocumentIndicator(false);
            return;
        }

        // Is it the flow sequence start indicator?
        if (analyzer.Check('[')) {
            FetchFlowCollectionStart(true);
            return;
        }

        // Is it the flow mapping start indicator?
        if (analyzer.Check('{')) {
            FetchFlowCollectionStart(false);
            return;
        }

        // Is it the flow sequence end indicator?
        if (analyzer.Check(']')) {
            FetchFlowCollectionEnd(true);
            return;
        }

        // Is it the flow mapping end indicator?
        if (analyzer.Check('}')) {
            FetchFlowCollectionEnd(false);
            return;
        }

        // Is it the flow entry indicator?
        if (analyzer.Check(',')) {
            FetchFlowEntry();
            return;
        }

        // Is it the block entry indicator?
        if (analyzer.Check('-') && analyzer.IsBlankOrBreakOrZero(1)) {
            FetchBlockEntry();
            return;
        }

        // Is it the key indicator?
        if (analyzer.Check('?') && (flowLevel > 0 || analyzer.IsBlankOrBreakOrZero(1))) {
            FetchKey();
            return;
        }

        // Is it the value indicator?
        if (analyzer.Check(':') && (flowLevel > 0 || analyzer.IsBlankOrBreakOrZero(1))) {
            FetchValue();
            return;
        }

        // Is it an alias?
        if (analyzer.Check('*')) {
            FetchAnchor(true);
            return;
        }

        // Is it an anchor?
        if (analyzer.Check('&')) {
            FetchAnchor(false);
            return;
        }

        // Is it a tag?
        if (analyzer.Check('!')) {
            FetchTag();
            return;
        }

        // Is it a literal scalar?
        if (analyzer.Check('|') && flowLevel == 0) {
            FetchBlockScalar(true);
            return;
        }

        // Is it a folded scalar?
        if (analyzer.Check('>') && flowLevel == 0) {
            FetchBlockScalar(false);
            return;
        }

        // Is it a single-quoted scalar?
        if (analyzer.Check('\'')) {
            FetchFlowScalar(true);
            return;
        }

        // Is it a double-quoted scalar?
        if (analyzer.Check('"')) {
            FetchFlowScalar(false);
            return;
        }

        // Is it a plain scalar?
        // A plain scalar may start with any non-blank characters except
        //      '-', '?', ':', ',', '[', ']', '{', '}',
        //      '#', '&', '*', '!', '|', '>', '\'', '\"',
        //      '%', '@', '`'.
        // In the block context (and, for the '-' indicator, in the flow context
        // too), it may also start with the characters
        //      '-', '?', ':'
        // if it is followed by a non-space character.
        // The last rule is more restrictive than the specification requires.

        var isInvalidPlainScalarCharacter = analyzer.IsBlankOrBreakOrZero() || analyzer.Check("-?:,[]{}#&*!|>'\"%@`");

        var isPlainScalar =
            !isInvalidPlainScalarCharacter
            || (analyzer.Check('-') && !analyzer.IsBlank(1))
            || (flowLevel == 0 && analyzer.Check("?:") && !analyzer.IsBlankOrBreakOrZero(1));

        if (isPlainScalar) {
            FetchPlainScalar();
            return;
        }

        // If we don't determine the token type so far, it is an error.
        throw new SyntaxErrorException(
            mark,
            mark,
            "While scanning for the next token, find character that cannot start any token."
        );
    }

    bool CheckWhiteSpace() => analyzer.Check(' ') || ((flowLevel > 0 || !simpleKeyAllowed) && analyzer.Check('\t'));

    bool IsDocumentIndicator() {
        if (mark.Column == 0 && analyzer.IsBlankOrBreakOrZero(3)) {
            var isDocumentStart = analyzer.Check('-', 0) && analyzer.Check('-', 1) && analyzer.Check('-', 2);
            var isDocumentEnd = analyzer.Check('.', 0) && analyzer.Check('.', 1) && analyzer.Check('.', 2);

            return isDocumentStart || isDocumentEnd;
        }

        return false;
    }

    void Skip() {
        ++mark.Index;
        ++mark.Column;
        analyzer.Buffer.Skip(1);
    }

    void SkipLine() {
        if (analyzer.IsCrLf()) {
            mark.Index += 2;
            mark.Column = 0;
            ++mark.Line;
            analyzer.Buffer.Skip(2);
        } else if (analyzer.IsBreak()) {
            ++mark.Index;
            mark.Column = 0;
            ++mark.Line;
            analyzer.Buffer.Skip(1);
        } else if (!analyzer.IsZero()) {
            throw new InvalidOperationException("Not at a break.");
        }
    }

    void ScanToNextToken() {
        // Until the next token is not find.

        for (;;) {
            // Eat whitespaces.
            // Tabs are allowed:
            //  - in the flow context;
            //  - in the block context, but not at the beginning of the line or
            //  after '-', '?', or ':' (complex value).

            while (CheckWhiteSpace()) {
                Skip();
            }

            // Eat a comment until a line break.
            if (analyzer.Check('#')) {
                while (!analyzer.IsBreakOrZero()) {
                    Skip();
                }
            }

            // If it is a line break, eat it.
            if (analyzer.IsBreak()) {
                SkipLine();

                // In the block context, a new line may start a simple key.
                if (flowLevel == 0) {
                    simpleKeyAllowed = true;
                }
            } else {
                // We have find a token.
                break;
            }
        }
    }

    void FetchStreamStart() {
        // Initialize the simple key stack.
        simpleKeys.Push(new());

        // A simple key is allowed at the beginning of the stream.
        simpleKeyAllowed = true;

        // We have started.
        streamStartProduced = true;

        // Create the STREAM-START token and append it to the queue.
        tokens.Enqueue(new StreamStart(mark, mark));
    }

    /// <summary>
    ///     Pop indentation levels from the indents stack until the current level
    ///     becomes less or equal to the column.  For each indentation level, append
    ///     the BLOCK-END token.
    /// </summary>
    void UnrollIndent(int column) {
        // In the flow context, do nothing.

        if (flowLevel != 0) {
            return;
        }

        // Loop through the indentation levels in the stack.
        while (indent > column) {
            // Create a token and append it to the queue.
            tokens.Enqueue(new BlockEnd(mark, mark));

            // Pop the indentation level.
            indent = indents.Pop();
        }
    }

    /// <summary>
    ///     Produce the STREAM-END token and shut down the scanner.
    /// </summary>
    void FetchStreamEnd() {
        // Force new line.
        if (mark.Column != 0) {
            mark.Column = 0;
            ++mark.Line;
        }

        // Reset the indentation level.
        UnrollIndent(-1);

        // Reset simple keys.
        RemoveSimpleKey();
        simpleKeyAllowed = false;

        // Create the STREAM-END token and append it to the queue.
        streamEndProduced = true;
        tokens.Enqueue(new StreamEnd(mark, mark));
    }

    void FetchDirective() {
        // Reset the indentation level.
        UnrollIndent(-1);

        // Reset simple keys.
        RemoveSimpleKey();
        simpleKeyAllowed = false;

        // Create the YAML-DIRECTIVE or TAG-DIRECTIVE token.
        var token = ScanDirective();

        // Append the token to the queue.
        tokens.Enqueue(token);
    }

    /// <summary>
    ///     Scan a YAML-DIRECTIVE or TAG-DIRECTIVE token.
    ///     Scope:
    ///     %YAML    1.1    # a comment \n
    ///     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    ///     %TAG    !yaml!  tag:yaml.org,2002:  \n
    ///     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    /// </summary>
    Token ScanDirective() {
        // Eat '%'.
        var start = mark;
        Skip();

        // Scan the directive name.
        var name = ScanDirectiveName(start);

        // Is it a YAML directive?
        var directive = name switch {
            "YAML" => ScanVersionDirectiveValue(start),
            "TAG" => ScanTagDirectiveValue(start),
            _ => throw new SyntaxErrorException(start, mark, "While scanning a directive, find unknown directive name.")
        };

        // Eat the rest of the line including any comments.
        while (analyzer.IsBlank()) {
            Skip();
        }

        if (analyzer.Check('#')) {
            while (!analyzer.IsBreakOrZero()) {
                Skip();
            }
        }

        // Check if we are at the end of the line.
        if (!analyzer.IsBreakOrZero()) {
            throw new SyntaxErrorException(
                start,
                mark,
                "While scanning a directive, did not find expected comment or line break."
            );
        }

        // Eat a line break.
        if (analyzer.IsBreak()) {
            SkipLine();
        }

        return directive;
    }

    /// <summary>
    ///     Produce the DOCUMENT-START or DOCUMENT-END token.
    /// </summary>
    void FetchDocumentIndicator(bool isStartToken) {
        // Reset the indentation level.
        UnrollIndent(-1);

        // Reset simple keys.
        RemoveSimpleKey();
        simpleKeyAllowed = false;

        // Consume the token.
        var start = mark;
        Skip();
        Skip();
        Skip();

        Token token = isStartToken ? new DocumentStart(start, mark) : new DocumentEnd(start, start);
        tokens.Enqueue(token);
    }

    /// <summary>
    ///     Produce the FLOW-SEQUENCE-START or FLOW-MAPPING-START token.
    /// </summary>
    void FetchFlowCollectionStart(bool isSequenceToken) {
        // The indicators '[' and '{' may start a simple key.
        SaveSimpleKey();

        // Increase the flow level.
        IncreaseFlowLevel();

        // A simple key may follow the indicators '[' and '{'.
        simpleKeyAllowed = true;

        // Consume the token.
        var start = mark;
        Skip();

        // Create the FLOW-SEQUENCE-START of FLOW-MAPPING-START token.
        Token token = isSequenceToken ? new FlowSequenceStart(start, start) : new FlowMappingStart(start, start);
        tokens.Enqueue(token);
    }

    /// <summary>
    ///     Increase the flow level and resize the simple key list if needed.
    /// </summary>
    void IncreaseFlowLevel() {
        // Reset the simple key on the next level.
        simpleKeys.Push(new());

        // Increase the flow level.
        ++flowLevel;
    }

    /// <summary>
    ///     Produce the FLOW-SEQUENCE-END or FLOW-MAPPING-END token.
    /// </summary>
    void FetchFlowCollectionEnd(bool isSequenceToken) {
        // Reset any potential simple key on the current flow level.
        RemoveSimpleKey();

        // Decrease the flow level.
        DecreaseFlowLevel();

        // No simple keys after the indicators ']' and '}'.
        simpleKeyAllowed = false;

        // Consume the token.
        var start = mark;
        Skip();

        Token token = isSequenceToken ? new FlowSequenceEnd(start, start) : new FlowMappingEnd(start, start);
        tokens.Enqueue(token);
    }

    /// <summary>
    ///     Decrease the flow level.
    /// </summary>
    void DecreaseFlowLevel() {
        Debug.Assert(flowLevel > 0, "Could flowLevel be zero when this method is called?");
        if (flowLevel > 0) {
            --flowLevel;
            simpleKeys.Pop();
        }
    }

    /// <summary>
    ///     Produce the FLOW-ENTRY token.
    /// </summary>
    void FetchFlowEntry() {
        // Reset any potential simple keys on the current flow level.
        RemoveSimpleKey();

        // Simple keys are allowed after ','.
        simpleKeyAllowed = true;

        // Consume the token.
        var start = mark;
        Skip();

        // Create the FLOW-ENTRY token and append it to the queue.
        tokens.Enqueue(new FlowEntry(start, mark));
    }

    /// <summary>
    ///     Produce the BLOCK-ENTRY token.
    /// </summary>
    void FetchBlockEntry() {
        // Check if the scanner is in the block context.
        if (flowLevel == 0) {
            // Check if we are allowed to start a new entry.
            if (!simpleKeyAllowed) {
                throw new SyntaxErrorException(mark, mark, "Block sequence entries are not allowed in this context.");
            }

            // Add the BLOCK-SEQUENCE-START token if needed.
            RollIndent(mark.Column, -1, true, mark);
        }

        // It is an error for the '-' indicator to occur in the flow context,
        // but we let the Parser detect and report about it because the Parser
        // is able to point to the context.
        // Reset any potential simple keys on the current flow level.
        RemoveSimpleKey();

        // Simple keys are allowed after '-'.
        simpleKeyAllowed = true;

        // Consume the token.
        var start = mark;
        Skip();

        // Create the BLOCK-ENTRY token and append it to the queue.
        tokens.Enqueue(new BlockEntry(start, mark));
    }

    /// <summary>
    ///     Produce the KEY token.
    /// </summary>
    void FetchKey() {
        // In the block context, additional checks are required.
        if (flowLevel == 0) {
            // Check if we are allowed to start a new key (not nessesary simple).
            if (!simpleKeyAllowed) {
                throw new SyntaxErrorException(mark, mark, "Mapping keys are not allowed in this context.");
            }

            // Add the BLOCK-MAPPING-START token if needed.
            RollIndent(mark.Column, -1, false, mark);
        }

        // Reset any potential simple keys on the current flow level.
        RemoveSimpleKey();

        // Simple keys are allowed after '?' in the block context.
        simpleKeyAllowed = flowLevel == 0;

        // Consume the token.
        var start = mark;
        Skip();

        // Create the KEY token and append it to the queue.
        tokens.Enqueue(new Key(start, mark));
    }

    /// <summary>
    ///     Produce the VALUE token.
    /// </summary>
    void FetchValue() {
        var simpleKey = simpleKeys.Peek();

        // Have we find a simple key?
        if (simpleKey.IsPossible) {
            // Create the KEY token and insert it into the queue.
            tokens.Insert(simpleKey.TokenNumber - tokensParsed, new Key(simpleKey.Mark, simpleKey.Mark));

            // In the block context, we may need to add the BLOCK-MAPPING-START token.
            RollIndent(simpleKey.Mark.Column, simpleKey.TokenNumber, false, simpleKey.Mark);

            // Remove the simple key.
            simpleKey.IsPossible = false;

            // A simple key cannot follow another simple key.
            simpleKeyAllowed = false;
        } else {
            // The ':' indicator follows a complex key.
            // In the block context, extra checks are required.
            if (flowLevel == 0) {
                // Check if we are allowed to start a complex value.
                if (!simpleKeyAllowed) {
                    throw new SyntaxErrorException(mark, mark, "Mapping values are not allowed in this context.");
                }

                // Add the BLOCK-MAPPING-START token if needed.
                RollIndent(mark.Column, -1, false, mark);
            }

            // Simple keys after ':' are allowed in the block context.
            simpleKeyAllowed = flowLevel == 0;
        }

        // Consume the token.
        var start = mark;
        Skip();

        // Create the VALUE token and append it to the queue.
        tokens.Enqueue(new Value(start, mark));
    }

    /// <summary>
    ///     Push the current indentation level to the stack and set the new level
    ///     the current column is greater than the indentation level.  In this case,
    ///     append or insert the specified token into the token queue.
    /// </summary>
    void RollIndent(int column, int number, bool isSequence, Mark position) {
        // In the flow context, do nothing.
        if (flowLevel > 0) {
            return;
        }

        if (indent < column) {
            // Push the current indentation level to the stack and set the new
            // indentation level.
            indents.Push(indent);
            indent = column;

            // Create a token and insert it into the queue.
            Token token = isSequence
                ? new BlockSequenceStart(position, position)
                : new BlockMappingStart(position, position);

            if (number == -1) {
                tokens.Enqueue(token);
            } else {
                tokens.Insert(number - tokensParsed, token);
            }
        }
    }

    /// <summary>
    ///     Produce the ALIAS or ANCHOR token.
    /// </summary>
    void FetchAnchor(bool isAlias) {
        // An anchor or an alias could be a simple key.
        SaveSimpleKey();

        // A simple key cannot follow an anchor or an alias.
        simpleKeyAllowed = false;

        // Create the ALIAS or ANCHOR token and append it to the queue.
        tokens.Enqueue(ScanAnchor(isAlias));
    }

    Token ScanAnchor(bool isAlias) {
        // Eat the indicator character.
        var start = mark;
        Skip();

        // Consume the value.
        var value = new StringBuilder();
        while (analyzer.IsAlpha()) {
            value.Append(ReadCurrentCharacter());
        }

        // Check if length of the anchor is greater than 0 and it is followed by
        // a whitespace character or one of the indicators:
        //      '?', ':', ',', ']', '}', '%', '@', '`'.

        if (value.Length == 0 || !(analyzer.IsBlankOrBreakOrZero() || analyzer.Check("?:,]}%@`"))) {
            throw new SyntaxErrorException(
                start,
                mark,
                "While scanning an anchor or alias, did not find expected alphabetic or numeric character."
            );
        }

        // Create a token.
        if (isAlias) {
            return new AnchorAlias(value.ToString(), start, mark);
        }

        return new Anchor(value.ToString(), start, mark);
    }

    /// <summary>
    ///     Produce the TAG token.
    /// </summary>
    void FetchTag() {
        // A tag could be a simple key.
        SaveSimpleKey();

        // A simple key cannot follow a tag.
        simpleKeyAllowed = false;

        // Create the TAG token and append it to the queue.
        tokens.Enqueue(ScanTag());
    }

    /// <summary>
    ///     Scan a TAG token.
    /// </summary>
    Token ScanTag() {
        var start = mark;

        // Check if the tag is in the canonical form.
        string handle;
        string suffix;

        if (analyzer.Check('<', 1)) {
            // Set the handle to ''
            handle = string.Empty;

            // Eat '!<'
            Skip();
            Skip();

            // Consume the tag value.
            suffix = ScanTagUri(null, start);

            // Check for '>' and eat it.
            if (!analyzer.Check('>')) {
                throw new SyntaxErrorException(start, mark, "While scanning a tag, did not find the expected '>'.");
            }

            Skip();
        } else {
            // The tag has either the '!suffix' or the '!handle!suffix' form.
            // First, try to scan a handle.
            var firstPart = ScanTagHandle(false, start);

            // Check if it is, indeed, handle.
            if (firstPart.Length > 1 && firstPart[0] == '!' && firstPart[^1] == '!') {
                handle = firstPart;

                // Scan the suffix now.
                suffix = ScanTagUri(null, start);
            } else {
                // It wasn't a handle after all.  Scan the rest of the tag.
                suffix = ScanTagUri(firstPart, start);

                // Set the handle to '!'.
                handle = "!";

                // A special case: the '!' tag.  Set the handle to '' and the
                // suffix to '!'.
                if (suffix.Length == 0) {
                    suffix = handle;
                    handle = string.Empty;
                }
            }
        }

        // Check the character which ends the tag.
        if (!analyzer.IsBlankOrBreakOrZero()) {
            throw new SyntaxErrorException(
                start,
                mark,
                "While scanning a tag, did not find expected whitespace or line break."
            );
        }

        // Create a token.
        return new Tag(handle, suffix, start, mark);
    }

    /// <summary>
    ///     Produce the SCALAR(...,literal) or SCALAR(...,folded) tokens.
    /// </summary>
    void FetchBlockScalar(bool isLiteral) {
        // Remove any potential simple keys.
        RemoveSimpleKey();

        // A simple key may follow a block scalar.
        simpleKeyAllowed = true;

        // Create the SCALAR token and append it to the queue.
        tokens.Enqueue(ScanBlockScalar(isLiteral));
    }

    /// <summary>
    ///     Scan a block scalar.
    /// </summary>
    Token ScanBlockScalar(bool isLiteral) {
        var value = new StringBuilder();
        var leadingBreak = new StringBuilder();
        var trailingBreaks = new StringBuilder();

        var chomping = 0;
        var increment = 0;
        var currentIndent = 0;
        var leadingBlank = false;

        // Eat the indicator '|' or '>'.
        var start = mark;
        Skip();

        // Check for a chomping indicator.
        if (analyzer.Check("+-")) {
            // Set the chomping method and eat the indicator.
            chomping = analyzer.Check('+') ? +1 : -1;
            Skip();

            // Check for an indentation indicator.
            if (analyzer.IsDigit()) {
                // Check that the indentation is greater than 0.
                if (analyzer.Check('0')) {
                    throw new SyntaxErrorException(
                        start,
                        mark,
                        "While scanning a block scalar, find an indentation indicator equal to 0."
                    );
                }

                // Get the indentation level and eat the indicator.
                increment = analyzer.AsDigit();
                Skip();
            }
        }

        // Do the same as above, but in the opposite order.
        else if (analyzer.IsDigit()) {
            if (analyzer.Check('0')) {
                throw new SyntaxErrorException(
                    start,
                    mark,
                    "While scanning a block scalar, find an indentation indicator equal to 0."
                );
            }

            increment = analyzer.AsDigit();
            Skip();

            if (analyzer.Check("+-")) {
                chomping = analyzer.Check('+') ? +1 : -1;
                Skip();
            }
        }

        // Eat whitespaces and comments to the end of the line.
        while (analyzer.IsBlank()) {
            Skip();
        }

        if (analyzer.Check('#')) {
            while (!analyzer.IsBreakOrZero()) {
                Skip();
            }
        }

        // Check if we are at the end of the line.
        if (!analyzer.IsBreakOrZero()) {
            throw new SyntaxErrorException(
                start,
                mark,
                "While scanning a block scalar, did not find expected comment or line break."
            );
        }

        // Eat a line break.
        if (analyzer.IsBreak()) {
            SkipLine();
        }

        var end = mark;

        // Set the indentation level if it was specified.
        if (increment != 0) {
            currentIndent = indent >= 0 ? indent + increment : increment;
        }

        // Scan the leading line breaks and determine the indentation level if needed.
        currentIndent = ScanBlockScalarBreaks(currentIndent, trailingBreaks, start, ref end);

        // Scan the block scalar content.
        while (mark.Column == currentIndent && !analyzer.IsZero()) {
            // We are at the beginning of a non-empty line.
            // Is it a trailing whitespace?
            var trailingBlank = analyzer.IsBlank();

            // Check if we need to fold the leading line break.
            if (
                !isLiteral
                && (StartsWith(leadingBreak, '\r') || StartsWith(leadingBreak, '\n'))
                && !leadingBlank
                && !trailingBlank
            ) {
                // Do we need to join the lines by space?
                if (trailingBreaks.Length == 0) {
                    value.Append(' ');
                }

                leadingBreak.Length = 0;
            } else {
                value.Append(leadingBreak.ToString());
                leadingBreak.Length = 0;
            }

            // Append the remaining line breaks.
            value.Append(trailingBreaks.ToString());
            trailingBreaks.Length = 0;

            // Is it a leading whitespace?
            leadingBlank = analyzer.IsBlank();

            // Consume the current line.
            while (!analyzer.IsBreakOrZero()) {
                value.Append(ReadCurrentCharacter());
            }

            // Consume the line break.
            ReadLine(leadingBreak);

            // Eat the following indentation spaces and line breaks.
            currentIndent = ScanBlockScalarBreaks(currentIndent, trailingBreaks, start, ref end);
        }

        // Chomp the tail.
        if (chomping != -1) {
            value.Append(leadingBreak);
        }

        if (chomping == 1) {
            value.Append(trailingBreaks);
        }

        // Create a token.
        var style = isLiteral ? ScalarStyle.Literal : ScalarStyle.Folded;
        return new Scalar(value.ToString(), style, start, end);
    }

    /// <summary>
    ///     Scan indentation spaces and line breaks for a block scalar.  Determine the
    ///     indentation level if needed.
    /// </summary>
    int ScanBlockScalarBreaks(int currentIndent, StringBuilder breaks, Mark start, ref Mark end) {
        var maxIndent = 0;
        end = mark;

        // Eat the indentation spaces and line breaks.
        for (;;) {
            // Eat the indentation spaces.
            while ((currentIndent == 0 || mark.Column < currentIndent) && analyzer.IsSpace()) {
                Skip();
            }

            if (mark.Column > maxIndent) {
                maxIndent = mark.Column;
            }

            // Check for a tab character messing the indentation.
            if ((currentIndent == 0 || mark.Column < currentIndent) && analyzer.IsTab()) {
                throw new SyntaxErrorException(
                    start,
                    mark,
                    "While scanning a block scalar, find a tab character where an indentation space is expected."
                );
            }

            // Have we find a non-empty line?
            if (!analyzer.IsBreak()) {
                break;
            }

            // Consume the line break.
            ReadLine(breaks);
            end = mark;
        }

        // Determine the indentation level if needed.
        if (currentIndent == 0) {
            currentIndent = Math.Max(maxIndent, Math.Max(indent + 1, 1));
        }

        return currentIndent;
    }

    /// <summary>
    ///     Produce the SCALAR(...,single-quoted) or SCALAR(...,double-quoted) tokens.
    /// </summary>
    void FetchFlowScalar(bool isSingleQuoted) {
        // A plain scalar could be a simple key.
        SaveSimpleKey();

        // A simple key cannot follow a flow scalar.
        simpleKeyAllowed = false;

        // Create the SCALAR token and append it to the queue.
        tokens.Enqueue(ScanFlowScalar(isSingleQuoted));
    }

    /// <summary>
    ///     Scan a quoted scalar.
    /// </summary>
    Token ScanFlowScalar(bool isSingleQuoted) {
        // Eat the left quote.
        var start = mark;
        var end = mark;

        Skip();

        // Consume the content of the quoted scalar.
        scanScalarValue.Clear();
        scanScalarWhitespaces.Clear();
        scanScalarLeadingBreak.Clear();
        scanScalarTrailingBreaks.Clear();
        for (;;) {
            // Check that there are no document indicators at the beginning of the line.
            if (IsDocumentIndicator()) {
                throw new SyntaxErrorException(
                    start,
                    mark,
                    "While scanning a quoted scalar, find unexpected document indicator."
                );
            }

            // Check for EOF.
            if (analyzer.IsZero()) {
                throw new SyntaxErrorException(
                    start,
                    mark,
                    "While scanning a quoted scalar, find unexpected end of stream."
                );
            }

            // Consume non-blank characters.
            var hasLeadingBlanks = false;

            while (!analyzer.IsBlankOrBreakOrZero()) {
                // Check for an escaped single quote.
                if (isSingleQuoted && analyzer.Check('\'', 0) && analyzer.Check('\'', 1)) {
                    scanScalarValue.Append('\'');
                    Skip();
                    Skip();
                }

                // Check for the right quote.
                else if (analyzer.Check(isSingleQuoted ? '\'' : '"')) {
                    break;
                }

                // Check for an escaped line break.
                else if (!isSingleQuoted && analyzer.Check('\\') && analyzer.IsBreak(1)) {
                    Skip();
                    SkipLine();
                    hasLeadingBlanks = true;
                    break;
                }

                // Check for an escape sequence.
                else if (!isSingleQuoted && analyzer.Check('\\')) {
                    var codeLength = 0;

                    // Check the escape character.
                    var escapeCharacter = analyzer.Peek(1);
                    switch (escapeCharacter) {
                        case 'x':
                            codeLength = 2;
                            break;

                        case 'u':
                            codeLength = 4;
                            break;

                        case 'U':
                            codeLength = 8;
                            break;

                        default:
                            if (simpleEscapeCodes.TryGetValue(escapeCharacter, out var unescapedCharacter)) {
                                scanScalarValue.Append(unescapedCharacter);
                            } else {
                                throw new SyntaxErrorException(
                                    start,
                                    mark,
                                    "While parsing a quoted scalar, find unknown escape character."
                                );
                            }

                            break;
                    }

                    Skip();
                    Skip();

                    // Consume an arbitrary escape code.
                    if (codeLength > 0) {
                        uint character = 0;

                        // Scan the character value.
                        for (var k = 0; k < codeLength; ++k) {
                            if (!analyzer.IsHex(k)) {
                                throw new SyntaxErrorException(
                                    start,
                                    mark,
                                    "While parsing a quoted scalar, did not find expected hexadecimal number."
                                );
                            }

                            character = (uint)((character << 4) + analyzer.AsHex(k));
                        }

                        // Check the value and write the character.
                        if (character is >= 0xD800 and <= 0xDFFF or > 0x10FFFF) {
                            throw new SyntaxErrorException(
                                start,
                                mark,
                                "While parsing a quoted scalar, find invalid Unicode character escape code."
                            );
                        }

                        scanScalarValue.Append((char)character);

                        // Advance the pointer.
                        for (var k = 0; k < codeLength; ++k) {
                            Skip();
                        }
                    }
                } else {
                    // It is a non-escaped non-blank character.
                    scanScalarValue.Append(ReadCurrentCharacter());
                }
            }

            // Check if we are at the end of the scalar.
            if (analyzer.Check(isSingleQuoted ? '\'' : '"')) {
                break;
            }

            // Consume blank characters.
            while (analyzer.IsBlank() || analyzer.IsBreak()) {
                if (analyzer.IsBlank()) {
                    // Consume a space or a tab character.
                    if (!hasLeadingBlanks) {
                        scanScalarWhitespaces.Append(ReadCurrentCharacter());
                    } else {
                        Skip();
                    }
                } else {
                    // Check if it is a first line break.
                    if (!hasLeadingBlanks) {
                        scanScalarWhitespaces.Length = 0;
                        ReadLine(scanScalarLeadingBreak);
                        hasLeadingBlanks = true;
                    } else {
                        ReadLine(scanScalarTrailingBreaks);
                    }
                }
            }

            // Join the whitespaces or fold line breaks.
            if (hasLeadingBlanks) {
                // Do we need to fold line breaks?
                if (StartsWith(scanScalarLeadingBreak, '\n')) {
                    if (scanScalarTrailingBreaks.Length == 0) {
                        scanScalarValue.Append(' ');
                    } else {
                        scanScalarValue.Append(scanScalarTrailingBreaks);
                    }
                } else {
                    scanScalarValue.Append(scanScalarLeadingBreak);
                    scanScalarValue.Append(scanScalarTrailingBreaks);
                }

                scanScalarLeadingBreak.Length = 0;
                scanScalarTrailingBreaks.Length = 0;
            } else {
                scanScalarValue.Append(scanScalarWhitespaces);
                scanScalarWhitespaces.Length = 0;
            }
        }

        // Eat the right quote.
        Skip();

        end = mark;
        return new Scalar(
            scanScalarValue.ToString(),
            isSingleQuoted ? ScalarStyle.SingleQuoted : ScalarStyle.DoubleQuoted,
            start,
            end
        );
    }

    /// <summary>
    ///     Produce the SCALAR(...,plain) token.
    /// </summary>
    void FetchPlainScalar() {
        // A plain scalar could be a simple key.
        SaveSimpleKey();

        // A simple key cannot follow a flow scalar.
        simpleKeyAllowed = false;

        // Create the SCALAR token and append it to the queue.
        tokens.Enqueue(ScanPlainScalar());
    }

    /// <summary>
    ///     Scan a plain scalar.
    /// </summary>
    Token ScanPlainScalar() {
        var value = new StringBuilder();
        var whitespaces = new StringBuilder();
        var leadingBreak = new StringBuilder();
        var trailingBreaks = new StringBuilder();

        var hasLeadingBlanks = false;
        var currentIndent = indent + 1;

        var start = mark;
        var end = mark;

        // Consume the content of the plain scalar.
        for (;;) {
            // Check for a document indicator.
            if (IsDocumentIndicator()) {
                break;
            }

            // Check for a comment.
            if (analyzer.Check('#')) {
                break;
            }

            // Consume non-blank characters.
            while (!analyzer.IsBlankOrBreakOrZero()) {
                // Check for 'x:x' in the flow context. TODO: Fix the test "spec-08-13".
                if (flowLevel > 0 && analyzer.Check(':') && !analyzer.IsBlankOrBreakOrZero(1)) {
                    throw new SyntaxErrorException(start, mark, "While scanning a plain scalar, find unexpected ':'.");
                }

                // Check for indicators that may end a plain scalar.
                if ((analyzer.Check(':') && analyzer.IsBlankOrBreakOrZero(1))
                    || (flowLevel > 0 && analyzer.Check(",:?[]{}"))) {
                    break;
                }

                // Check if we need to join whitespaces and breaks.
                if (hasLeadingBlanks || whitespaces.Length > 0) {
                    if (hasLeadingBlanks) {
                        // Do we need to fold line breaks?
                        if (StartsWith(leadingBreak, '\n')) {
                            if (trailingBreaks.Length == 0) {
                                value.Append(' ');
                            } else {
                                value.Append(trailingBreaks);
                            }
                        } else {
                            value.Append(leadingBreak);
                            value.Append(trailingBreaks);
                        }

                        leadingBreak.Length = 0;
                        trailingBreaks.Length = 0;
                        hasLeadingBlanks = false;
                    } else {
                        value.Append(whitespaces);
                        whitespaces.Length = 0;
                    }
                }

                // Copy the character.
                value.Append(ReadCurrentCharacter());
                end = mark;
            }

            // Is it the end?
            if (!(analyzer.IsBlank() || analyzer.IsBreak())) {
                break;
            }

            // Consume blank characters.
            while (analyzer.IsBlank() || analyzer.IsBreak()) {
                if (analyzer.IsBlank()) {
                    // Check for tab character that abuse indentation.

                    if (hasLeadingBlanks && mark.Column < currentIndent && analyzer.IsTab()) {
                        throw new SyntaxErrorException(
                            start,
                            mark,
                            "While scanning a plain scalar, find a tab character that violate indentation."
                        );
                    }

                    // Consume a space or a tab character.
                    if (!hasLeadingBlanks) {
                        whitespaces.Append(ReadCurrentCharacter());
                    } else {
                        Skip();
                    }
                } else {
                    // Check if it is a first line break.
                    if (!hasLeadingBlanks) {
                        whitespaces.Length = 0;
                        ReadLine(leadingBreak);
                        hasLeadingBlanks = true;
                    } else {
                        ReadLine(trailingBreaks);
                    }
                }
            }

            // Check indentation level.
            if (flowLevel == 0 && mark.Column < currentIndent) {
                break;
            }
        }

        // Note that we change the 'simple_key_allowed' flag.
        if (hasLeadingBlanks) {
            simpleKeyAllowed = true;
        }

        // Create a token.
        return new Scalar(value.ToString(), ScalarStyle.Plain, start, end);
    }

    /// <summary>
    ///     Remove a potential simple key at the current flow level.
    /// </summary>
    void RemoveSimpleKey() {
        var key = simpleKeys.Peek();

        if (key is { IsPossible: true, IsRequired: true }) {
            // If the key is required, it is an error.
            throw new SyntaxErrorException(
                key.Mark,
                key.Mark,
                "While scanning a simple key, could not find expected ':'."
            );
        }

        // Remove the key from the stack.
        key.IsPossible = false;
    }

    /// <summary>
    ///     Scan the directive name.
    ///     Scope:
    ///     %YAML   1.1     # a comment \n
    ///     ^^^^
    ///     %TAG    !yaml!  tag:yaml.org,2002:  \n
    ///     ^^^
    /// </summary>
    string ScanDirectiveName(Mark start) {
        var name = new StringBuilder();

        // Consume the directive name.
        while (analyzer.IsAlpha()) {
            name.Append(ReadCurrentCharacter());
        }

        // Check if the name is empty.
        if (name.Length == 0) {
            throw new SyntaxErrorException(
                start,
                mark,
                "While scanning a directive, could not find expected directive name."
            );
        }

        // Check for an blank character after the name.
        if (!analyzer.IsBlankOrBreakOrZero()) {
            throw new SyntaxErrorException(
                start,
                mark,
                "While scanning a directive, find unexpected non-alphabetical character."
            );
        }

        return name.ToString();
    }

    void SkipWhitespaces() {
        // Eat whitespaces.
        while (analyzer.IsBlank()) {
            Skip();
        }
    }

    /// <summary>
    ///     Scan the value of VERSION-DIRECTIVE.
    ///     Scope:
    ///     %YAML   1.1     # a comment \n
    ///     ^^^^^^
    /// </summary>
    Token ScanVersionDirectiveValue(Mark start) {
        SkipWhitespaces();

        // Consume the major version number.
        var major = ScanVersionDirectiveNumber(start);

        // Eat '.'.
        if (!analyzer.Check('.')) {
            throw new SyntaxErrorException(
                start,
                mark,
                "While scanning a %YAML directive, did not find expected digit or '.' character."
            );
        }

        Skip();

        // Consume the minor version number.
        var minor = ScanVersionDirectiveNumber(start);
        return new VersionDirective(new(major, minor), start, start);
    }

    /// <summary>
    ///     Scan the value of a TAG-DIRECTIVE token.
    ///     Scope:
    ///     %TAG    !yaml!  tag:yaml.org,2002:  \n
    ///     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    /// </summary>
    Token ScanTagDirectiveValue(Mark start) {
        SkipWhitespaces();

        // Scan a handle.
        var handle = ScanTagHandle(true, start);

        // Expect a whitespace.
        if (!analyzer.IsBlank()) {
            throw new SyntaxErrorException(
                start,
                mark,
                "While scanning a %TAG directive, did not find expected whitespace."
            );
        }

        SkipWhitespaces();

        // Scan a prefix.
        var prefix = ScanTagUri(null, start);

        // Expect a whitespace or line break.
        if (!analyzer.IsBlankOrBreakOrZero()) {
            throw new SyntaxErrorException(
                start,
                mark,
                "While scanning a %TAG directive, did not find expected whitespace or line break."
            );
        }

        return new TagDirective(handle, prefix, start, start);
    }

    /// <summary>
    ///     Scan a tag.
    /// </summary>
    string ScanTagUri(string? head, Mark start) {
        var tag = new StringBuilder();
        if (head is { Length: > 1 }) {
            tag.Append(head[1..]);
        }

        // Scan the tag.
        // The set of characters that may appear in URI is as follows:
        //      '0'-'9', 'A'-'Z', 'a'-'z', '_', '-', ';', '/', '?', ':', '@', '&',
        //      '=', '+', '$', ',', '.', '!', '~', '*', '\'', '(', ')', '[', ']',
        //      '%'.
        while (analyzer.IsAlpha() || analyzer.Check(";/?:@&=+$,.!~*'()[]%")) {
            // Check if it is a URI-escape sequence.
            if (analyzer.Check('%')) {
                tag.Append(ScanUriEscapes(start));
            } else {
                tag.Append(ReadCurrentCharacter());
            }
        }

        // Check if the tag is non-empty.
        if (tag.Length == 0) {
            throw new SyntaxErrorException(start, mark, "While parsing a tag, did not find expected tag URI.");
        }

        return tag.ToString();
    }

    /// <summary>
    ///     Decode an URI-escape sequence corresponding to a single UTF-8 character.
    /// </summary>
    char ScanUriEscapes(Mark start) {
        // Decode the required number of characters.
        var charBytes = new List<byte>();
        var width = 0;
        do {
            // Check for a URI-escaped octet.
            if (!(analyzer.Check('%') && analyzer.IsHex(1) && analyzer.IsHex(2))) {
                throw new SyntaxErrorException(start, mark, "While parsing a tag, did not find URI escaped octet.");
            }

            // Get the octet.
            var octet = (analyzer.AsHex(1) << 4) + analyzer.AsHex(2);

            // If it is the leading octet, determine the length of the UTF-8 sequence.
            if (width == 0) {
                width = (octet & 0x80) == 0x00 ? 1 :
                    (octet & 0xE0) == 0xC0 ? 2 :
                    (octet & 0xF0) == 0xE0 ? 3 :
                    (octet & 0xF8) == 0xF0 ? 4 : 0;

                if (width == 0) {
                    throw new SyntaxErrorException(
                        start,
                        mark,
                        "While parsing a tag, find an incorrect leading UTF-8 octet."
                    );
                }
            } else {
                // Check if the trailing octet is correct.
                if ((octet & 0xC0) != 0x80) {
                    throw new SyntaxErrorException(
                        start,
                        mark,
                        "While parsing a tag, find an incorrect trailing UTF-8 octet."
                    );
                }
            }

            // Copy the octet and move the pointers.
            charBytes.Add((byte)octet);

            Skip();
            Skip();
            Skip();
        } while (--width > 0);

        var characters = Encoding.UTF8.GetChars(charBytes.ToArray());
        if (characters.Length != 1) {
            throw new SyntaxErrorException(start, mark, "While parsing a tag, find an incorrect UTF-8 sequence.");
        }

        return characters[0];
    }

    /// <summary>
    ///     Scan a tag handle.
    /// </summary>
    string ScanTagHandle(bool isDirective, Mark start) {
        // Check the initial '!' character.
        if (!analyzer.Check('!')) {
            throw new SyntaxErrorException(start, mark, "While scanning a tag, did not find expected '!'.");
        }

        // Copy the '!' character.
        var tagHandle = new StringBuilder();
        tagHandle.Append(ReadCurrentCharacter());

        // Copy all subsequent alphabetical and numerical characters.
        while (analyzer.IsAlpha()) {
            tagHandle.Append(ReadCurrentCharacter());
        }

        // Check if the trailing character is '!' and copy it.
        if (analyzer.Check('!')) {
            tagHandle.Append(ReadCurrentCharacter());
        } else {
            // It's either the '!' tag or not really a tag handle.  If it's a %TAG
            // directive, it's an error.  If it's a tag token, it must be a part of
            // URI.
            if (isDirective && tagHandle is not ['!']) {
                throw new SyntaxErrorException(
                    start,
                    mark,
                    "While parsing a tag directive, did not find expected '!'."
                );
            }
        }

        return tagHandle.ToString();
    }

    /// <summary>
    ///     Scan the version number of VERSION-DIRECTIVE.
    ///     Scope:
    ///     %YAML   1.1     # a comment \n
    ///     ^
    ///     %YAML   1.1     # a comment \n
    ///     ^
    /// </summary>
    int ScanVersionDirectiveNumber(Mark start) {
        var value = 0;
        var length = 0;

        // Repeat while the next character is digit.
        while (analyzer.IsDigit()) {
            // Check if the number is too long.
            if (++length > MaxVersionNumberLength) {
                throw new SyntaxErrorException(
                    start,
                    mark,
                    "While scanning a %YAML directive, find extremely long version number."
                );
            }

            value = value * 10 + analyzer.AsDigit();
            Skip();
        }

        // Check if the number was present.
        if (length == 0) {
            throw new SyntaxErrorException(
                start,
                mark,
                "While scanning a %YAML directive, did not find expected version number."
            );
        }

        return value;
    }

    /// <summary>
    ///     Check if a simple key may start at the current position and add it if
    ///     needed.
    /// </summary>
    void SaveSimpleKey() {
        // A simple key is required at the current position if the scanner is in
        // the block context and the current column coincides with the indentation
        // level.
        var isRequired = flowLevel == 0 && indent == mark.Column;

        // A simple key is required only when it is the first token in the current
        // line.  Therefore it is always allowed.  But we add a check anyway.
        Debug.Assert(
            simpleKeyAllowed || !isRequired,
            "Can't require a simple key and disallow it at the same time."
        ); // Impossible.

        // If the current position may start a simple key, save it.
        if (simpleKeyAllowed) {
            var key = new SimpleKey(true, isRequired, tokensParsed + tokens.Count, mark);

            RemoveSimpleKey();
            simpleKeys.Pop();
            simpleKeys.Push(key);
        }
    }

    internal bool InternalMoveNext() {
        if (!tokenAvailable && !streamEndProduced) {
            FetchMoreTokens();
        }

        if (tokens.Count > 0) {
            Current = tokens.Dequeue();
            tokenAvailable = false;
            return true;
        }

        Current = null;
        return false;
    }

    /// <summary>
    ///     Consumes the current token and increments the parsed token count
    /// </summary>
    internal void ConsumeCurrent() {
        ++tokensParsed;
        tokenAvailable = false;
        Current = null;
    }
}
