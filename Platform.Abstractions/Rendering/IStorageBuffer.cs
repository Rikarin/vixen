namespace Rin.Platform.Abstractions.Rendering;

public interface IStorageBuffer : IDisposable {
    public void SetData(ReadOnlySpan<byte> data);
    public void SetData_RT(ReadOnlySpan<byte> data);
    public void Resize(int newSize);
}
