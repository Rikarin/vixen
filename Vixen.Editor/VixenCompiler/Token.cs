namespace Vixen.Editor.VixenCompiler;

public record struct Token(TokenType Type, Name Name, Location Location);
