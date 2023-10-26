namespace Rin.Editor.RinCompiler;

public record struct Position(int Value) {
    public static Position Zero = new(0);
    public Position GetWithOffset(int offset) => new(Value + offset);
}
