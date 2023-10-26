namespace Rin.Platform.Abstractions.Rendering;

public sealed class VertexBufferLayout {
    readonly List<VertexBufferElement> elements = new();

    public int Stride { get; private set; }
    public IReadOnlyList<VertexBufferElement> Elements => elements.AsReadOnly();
    public bool HasElements => elements.Count > 0;

    public VertexBufferLayout() { }

    public VertexBufferLayout(params VertexBufferElement[] elements) {
        this.elements.AddRange(elements);
        CalculateOffsetsAndStride();
    }

    void CalculateOffsetsAndStride() {
        int offset = 0;
        Stride = 0;

        foreach (var x in elements) {
            x.Offset = offset;
            offset += x.Size;
            Stride += x.Size;
        }
    }
}
