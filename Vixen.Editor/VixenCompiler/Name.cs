namespace Vixen.Editor.VixenCompiler;

public record struct Name(string Value) {
    public static Name Empty = new(string.Empty);
}
