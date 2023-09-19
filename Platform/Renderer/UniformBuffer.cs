using Rin.Platform.Silk;

namespace Rin.Platform.Renderer;

abstract class UniformBuffer : IDisposable {
    public abstract void SetData<T>(ReadOnlySpan<T> data, IntPtr offset = 0) where T : unmanaged;

    public static UniformBuffer Create(UIntPtr size, uint binding) {
        switch (RendererApi.CurrentApi) {
            case RendererApi.Api.None: throw new NotImplementedException();
            case RendererApi.Api.OpenGl: return new OpenGLUniformBuffer(size, binding);
            default: throw new ArgumentOutOfRangeException();
        }
    }

    public virtual void Dispose() { }
}
