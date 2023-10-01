namespace Rin.Editor.RinCompiler;

public record struct Token(TokenType Type, Name Name, Location Location);
