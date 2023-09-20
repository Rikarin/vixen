namespace Rin.Platform.Renderer;

public sealed class BufferLayout {
    readonly List<BufferElement> elements = new();

    public uint Stride { get; private set; }
    public IReadOnlyList<BufferElement> Elements => elements.AsReadOnly();

    public BufferLayout() { }

    public BufferLayout(IEnumerable<BufferElement> elements) {
        this.elements.AddRange(elements);
        CalculateOffsetsAndStride();
    }

    void CalculateOffsetsAndStride() {
        uint offset = 0;
        Stride = 0;

        foreach (var x in elements) {
            x.Offset = offset;
            offset += x.Size;
            Stride += x.Size;
        }
    }
}
