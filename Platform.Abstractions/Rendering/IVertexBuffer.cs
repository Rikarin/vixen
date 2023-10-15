using Rin.Core.Abstractions;

namespace Rin.Platform.Abstractions.Rendering;

public interface IVertexBuffer : IDisposable {
    RendererId RendererId { get; }
    int Size { get; }

    // TODO: consider using this API instead
    // public abstract void SetData<T>(ReadOnlySpan<T> data) where T : unmanaged;
    void SetData(ReadOnlySpan<byte> data);
    void SetData_RT(ReadOnlySpan<byte> data);
}
