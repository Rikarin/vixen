using Rin.Platform.Renderer;
using Silk.NET.OpenGL;

namespace Rin.Platform.Silk;

sealed class OpenGLUniformBuffer : UniformBuffer {
    readonly GL gl;
    readonly uint handle;

    public unsafe OpenGLUniformBuffer(UIntPtr size, uint binding) {
        gl = SilkWindow.MainWindow.Gl; // TODO: rewire this
        handle = gl.CreateBuffer();

        gl.NamedBufferData(handle, size, null, GLEnum.DynamicDraw);
        gl.BindBufferBase(GLEnum.UniformBuffer, binding, handle);
    }

    public override void SetData<T>(ReadOnlySpan<T> data, IntPtr offset = 0) {
        gl.NamedBufferSubData(handle, offset, data);
    }

    public override void Dispose() {
        base.Dispose();
        gl.DeleteBuffer(handle);
    }
}
