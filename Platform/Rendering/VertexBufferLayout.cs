namespace Rin.Platform.Rendering;

public sealed class VertexBufferLayout {
    readonly List<VertexBufferElement> elements = new();

    public uint Stride { get; private set; }
    public IReadOnlyList<VertexBufferElement> Elements => elements.AsReadOnly();

    public VertexBufferLayout() { }

    public VertexBufferLayout(IEnumerable<VertexBufferElement> elements) {
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
