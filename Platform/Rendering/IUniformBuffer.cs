namespace Rin.Platform.Rendering;

public interface IUniformBuffer : IDisposable {
    public void SetData(ReadOnlySpan<byte> data);
    public void SetData_RT(ReadOnlySpan<byte> data);
}