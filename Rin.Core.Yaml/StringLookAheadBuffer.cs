namespace Rin.Core.Yaml;

class StringLookAheadBuffer(string value) : ILookAheadBuffer {
    public int Length => value.Length;

    public int Position { get; private set; }

    public bool EndOfInput => IsOutside(Position);

    public char Peek(int offset) {
        var index = Position + offset;
        return IsOutside(index) ? '\0' : value[index];
    }

    public void Skip(int length) {
        if (length < 0) {
            throw new ArgumentOutOfRangeException(nameof(length), "The length must be positive.");
        }

        Position += length;
    }

    bool IsOutside(int index) => index >= value.Length;
}
