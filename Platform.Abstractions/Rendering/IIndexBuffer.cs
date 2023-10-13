namespace Rin.Platform.Abstractions.Rendering;

public interface IIndexBuffer : IDisposable {
    public int Count { get; }
    public int Size { get; }

    public void SetData(ReadOnlySpan<byte> data);
}
