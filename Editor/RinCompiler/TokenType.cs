namespace Rin.Editor.RinCompiler; 

public enum TokenType {
    Invalid,
    WhiteSpace,
    
    Begin,
    End,
    
    // Literals
    Identifier,
    StringLiteral,
    CharacterLiteral,
    IntegerLiteral,
    FloatLiteral,
    HlslProgram,
    
    // Keywords
    Shader, Properties, Pass, Tags, Lod, SubShader, Fallback,
    Tex2D, Tex3D, Tex2DArray, // 2D, 3D, 2DArray
    Int, Float, Range, Cube, CubeArray, Color, Vector,
    
    // Operators
    Slash,              // /
    SlashEqual,         // /=
    Dot,                // .
    DotDot,             // ..
    DotDotDot,          // ...
    Ampersand,          // &
    AmpersandEqual,     // &=
    AmpersandAmpersand, // &&
    Pipe,               // |
    PipeEqual,          // |=
    PipePipe,           // ||
    Minus,              // -
    MinusEqual,         // -=
    MinusMinus,         // --
    Plus,               // +
    PlusEqual,          // +=
    PlusPlus,           // ++
    Less,               // <
    LessEqual,          // <=
    LessLess,           // <<
    LessLessEqual,      // <<=
    More,               // >
    MoreEqual,          // >=
    MoreMore,           // >>
    MoreMoreMore,       // >>>
    MoreMoreEqual,      // >>=
    MoreMoreMoreEqual,  // >>>=
    Bang,               // !
    BangEqual,          // !=
    BangLess,           // !<
    BangLessEqual,      // !<=
    BangMore,           // !>
    BangMoreEqual,      // !>=
    LessMoreEqual,      // <>=
    BangLessMoreEqual,  // !<>=
    LessMore,           // <>
    BangLessMore,       // !<>
    OpenParen,          // (
    CloseParen,         // )
    OpenBracket,        // [
    CloseBracket,       // ]
    OpenBrace,          // {
    CloseBrace,         // }
    QuestionMark,       // ?
    QuestionMarkQuestionMark, // ??
    QuestionMarkDot,    // ?.
    QuestionMarkOpenBracket, // ?[
    Comma,              // ,
    Semicolon,          // ;
    Colon,              // :
    MinusMore,          // ->
    Dollar,             // $
    Equal,              // =
    EqualEqual,         // ==
    Asterisk,           // *
    AsteriskEqual,      // *=
    Percent,            // %
    PercentEqual,       // %=
    Caret,              // ^
    CaretEqual,         // ^=
    CaretCaret,         // ^^
    CaretCaretEqual,    // ^^=
    Tilde,              // ~
    TildeEqual,         // ~=
    //At,                 // @ TODO, do I need this symbol?
    EqualMore,          // =>
    Sharp,              // #
}