using Rin.Core.Abstractions;

namespace Rin.Platform.Rendering;

public interface IVertexBuffer : IDisposable {
    public RendererId RendererId { get; }
    public int Size { get; }

    // TODO: consider using this API instead
    // public abstract void SetData<T>(ReadOnlySpan<T> data) where T : unmanaged;
    public void SetData(ReadOnlySpan<byte> data);
    public void SetData_RT(ReadOnlySpan<byte> data);
}
