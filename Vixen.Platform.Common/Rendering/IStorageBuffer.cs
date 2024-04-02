namespace Vixen.Platform.Common.Rendering;

public interface IStorageBuffer : IDisposable {
    void SetData(ReadOnlySpan<byte> data);
    void SetData_RT(ReadOnlySpan<byte> data);
    void Resize(int newSize);
}
