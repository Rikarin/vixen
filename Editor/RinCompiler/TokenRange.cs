using Serilog;
using System.Diagnostics;
using System.Text;

namespace Rin.Editor.RinCompiler;

public class TokenRange {
    readonly Dictionary<string, Func<int, Token>> lexerMap = new();

    readonly string content;
    int index;
    Position basePos;
    Position prevPos;

    public Token Front { get; private set; } = new(TokenType.Begin, Name.Empty, Location.Zero);
    public Position Previous => prevPos;
    char FrontChar => content[index];
    public bool IsEmpty => Front.Type == TokenType.End;

    public TokenRange(string content) {
        this.content = content;
        CreateLexerMap();
    }

    public void PopChar(int count = 1) => index += count;

    public void PopFront() {
        prevPos = basePos.GetWithOffset(index);
        Front = GetNextToken();
    }

    public TokenRange Clone() => new(content) { index = index, basePos = basePos, prevPos = prevPos, Front = Front };

    public void Match(TokenType type) {
        if (Front.Type != type) {
            throw new($"Expected '{type}' got '{Front.Type}'");
        }

        PopFront();
    }

    Token GetNextToken() {
        Token? token;

        do {
            Lexer(index, out token);
            token ??= LexIdentifier();
        } while (token.Value.Type == TokenType.WhiteSpace);

        return token.Value;
    }

    void CreateLexerMap() {
        // Whitespaces
        lexerMap[" "] = LexWhiteSpace;
        lexerMap["\t"] = LexWhiteSpace;
        lexerMap["\v"] = LexWhiteSpace;
        lexerMap["\f"] = LexWhiteSpace;
        lexerMap["\n"] = LexWhiteSpace;
        lexerMap["\r"] = LexWhiteSpace;
        lexerMap["\r\n"] = LexWhiteSpace;

        // Comments
        lexerMap["//"] = LexComment;
        lexerMap["/+"] = LexComment;
        lexerMap["/*"] = LexComment;

        // Integer Literals
        lexerMap["0x"] = LexNumeric;
        lexerMap["0b"] = LexNumeric;

        // String Literals
        lexerMap["`"] = LexString;
        lexerMap["r\""] = LexString;
        lexerMap["\""] = LexString;
        lexerMap["x\""] = LexString;

        lexerMap["HLSLPROGRAM"] = LexProgram;
        lexerMap["'"] = LexChar;

        foreach (var keyword in TokenTypeMap.Keywords) {
            lexerMap[keyword.Key] = LexKeyword;
        }

        foreach (var op in TokenTypeMap.Operators) {
            lexerMap[op.Key] = _ => LexOperator(op.Key, op.Value);
        }

        for (var i = 0; i < 10; i++) {
            lexerMap[i.ToString()] = LexNumeric;
        }
    }

    Token LexOperator(string key, TokenType type) =>
        new(
            type,
            new(key),
            new(basePos.GetWithOffset(index - key.Length), basePos.GetWithOffset(index))
        );

    Token LexKeyword(int startIndex) {
        var keyword = content.Substring(startIndex, index - startIndex);
        var type = TokenTypeMap.Keywords[keyword];

        return new(type, new(keyword), new(basePos.GetWithOffset(startIndex), basePos.GetWithOffset(index)));
    }

    Token LexWhiteSpace(int _) => new(TokenType.WhiteSpace, Name.Empty, Location.Zero);

    Token LexComment(int _) => throw new NotImplementedException();

    Token LexNumeric(int _) {
        var c = FrontChar;
        var iBegin = index - 1;
        var begin = basePos.GetWithOffset(iBegin);

        while (true) {
            while (c is >= '0' and <= '9') {
                PopChar();
                c = FrontChar;
            }

            if (c == '_') {
                PopChar();
                c = FrontChar;
                continue;
            }

            break;
        }

        switch (c) {
            case '.':
                var lookAhead = Clone();
                lookAhead.PopFront();

                if (char.IsDigit(FrontChar)) {
                    PopChar();
                    throw new("Floating point not supported, yet");
                }

                break;

            case 'U':
                PopChar();
                c = FrontChar;
                if (c == 'L') {
                    PopChar();
                }

                break;

            case 'L':
                PopChar();
                c = FrontChar;
                if (c == 'U') {
                    PopChar();
                }

                break;
        }

        return new(
            TokenType.IntegerLiteral,
            new(content.Substring(iBegin, index - iBegin)),
            new(begin, basePos.GetWithOffset(index))
        );
    }

    Token LexString(int _) {
        var begin = basePos.GetWithOffset(index - 1);
        var buffer = new StringBuilder();

        while (FrontChar != '\"') {
            buffer.Append(FrontChar);
            PopChar();
        }

        PopChar();
        return new(TokenType.StringLiteral, new(buffer.ToString()), new(begin, basePos.GetWithOffset(index)));
    }

    Token LexChar(int _) {
        var begin = basePos.GetWithOffset(index - 1);
        var name = new Name(LexEscapeChar().ToString());
        if (FrontChar != '\'') {
            throw new("In '' must be character literal only");
        }

        PopChar();
        return new(TokenType.CharacterLiteral, name, new(begin, basePos.GetWithOffset(index)));
    }

    Token LexProgram(int _) {
        var begin = basePos.GetWithOffset(index - 1);
        var buffer = new StringBuilder();

        while (content[index..(index + 7)] != "ENDHLSL") {
            buffer.Append(FrontChar);
            PopChar();
        }

        PopChar(7);
        return new(TokenType.HlslProgram, new(buffer.ToString()), new(begin, basePos.GetWithOffset(index)));
    }

    // Internal lexers
    Token LexIdentifier() {
        var iBegin = index;
        var begin = basePos.GetWithOffset(iBegin);

        while (IsIdChar(FrontChar)) {
            PopChar();
        }

        return new(
            TokenType.Identifier,
            new(content.Substring(iBegin, index - iBegin)),
            new(begin, basePos.GetWithOffset(index))
        );
    }

    char LexEscapeSequence() {
        Debug.Assert(FrontChar == '\\', "Not a valid escape sequence");
        PopChar();
        var c = FrontChar;
        PopChar();

        return c switch {
            '\'' => '\'',
            '"' => '\"',
            '0' => '\0',
            'a' => '\a',
            'b' => '\b',
            'f' => '\f',
            'r' => '\r',
            'n' => '\n',
            't' => '\t',
            'v' => '\v',
            _ => throw new("Invalid escape sequence")
        };
    }

    char LexEscapeChar() {
        var c = FrontChar;
        Log.Information("Debug: {Variable}", c);
        switch (c) {
            case '\0': throw new("Unexpected EOF");
            case '\\': return LexEscapeSequence();
            case '\'': throw new("Empty character literal");
        }

        if ((c & 0x80) == 0x80) {
            throw new("Unicode not supported");
        }

        PopChar();
        return c;
    }


    bool Lexer(int startIndex, out Token? token) {
        // Range [iBegin .. index (including)]
        if (index >= content.Length) {
            token = new Token(TokenType.End, Name.Empty, Location.Zero);
            return false;
        }

        var str = content.Substring(startIndex, index - startIndex + 1);

        // Log.Information("ibegin: {Variable} index {index}", startIndex, index);
        // Log.Information("Base: {Base} | Front: {Front}", str, FrontChar);

        if (lexerMap.Keys.Any(key => key.StartsWith(str))) {
            PopChar();

            if (!Lexer(startIndex, out token)) {
                if (lexerMap.TryGetValue(str, out var lexerMethod)) {
                    token = lexerMethod(startIndex);
                }
            }

            return true;
        }

        token = null;
        return false;
    }

    static bool IsIdChar(char c) => c == '_' || char.IsDigit(c) || char.IsLetter(c);
}
