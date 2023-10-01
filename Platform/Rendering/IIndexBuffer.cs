namespace Rin.Platform.Rendering;

public interface IIndexBuffer : IDisposable {
    public int Count { get; }
    public int Size { get; }
    
    public void SetData(ReadOnlySpan<byte> data);
}

    // public abstract RendererId RendererId { get; }
    // public abstract void Bind();
