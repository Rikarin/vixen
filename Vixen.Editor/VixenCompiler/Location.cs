namespace Vixen.Editor.VixenCompiler;

public record struct Location(Position Start, Position End) {
    public static Location Zero = new(Position.Zero, Position.Zero);

    public Location SpanTo(Position end) => this with { End = end };
}
