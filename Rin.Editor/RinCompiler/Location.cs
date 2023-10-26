namespace Rin.Editor.RinCompiler;

public record struct Location(Position Start, Position End) {
    public static Location Zero = new(Position.Zero, Position.Zero);

    public Location SpanTo(Position end) => this with { End = end };
}
