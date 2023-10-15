namespace Rin.Platform.Abstractions.Rendering;

public interface IUniformBuffer : IDisposable {
    void SetData<T>(ReadOnlySpan<T> data) where T : struct;
    void SetData<T>(T data) where T : struct;

    void SetData_RT<T>(ReadOnlySpan<T> data) where T : struct;
    void SetData_RT<T>(T data) where T : struct;
}
