using Rin.Platform.Renderer;
using Silk.NET.OpenGL;

namespace Rin.Platform.Silk;

sealed class OpenGLVertexBuffer : VertexBuffer {
    readonly GL gl;
    readonly uint handle;

    public OpenGLVertexBuffer(uint size) {
        gl = SilkWindow.MainWindow.Gl; // TODO: rewire this
        handle = gl.CreateBuffer();

        gl.BindBuffer(GLEnum.ArrayBuffer, handle);

        unsafe {
            gl.BufferData(GLEnum.ArrayBuffer, size, null, GLEnum.DynamicDraw);
        }
    }

    public OpenGLVertexBuffer(ReadOnlySpan<uint> vertices) {
        gl = SilkWindow.MainWindow.Gl; // TODO: rewire this
        handle = gl.CreateBuffer();

        gl.BindBuffer(GLEnum.ArrayBuffer, handle);
        gl.BufferData(GLEnum.ArrayBuffer, vertices, GLEnum.StaticDraw);
    }

    public override void Bind() {
        gl.BindBuffer(GLEnum.ArrayBuffer, handle);
    }

    public override void Unbind() {
        gl.BindBuffer(GLEnum.ArrayBuffer, 0);
    }

    public override void SetData(ReadOnlySpan<uint> data) {
        Bind();
        gl.BufferSubData(GLEnum.ArrayBuffer, 0, data);
    }

    public override void Dispose() {
        base.Dispose();
        gl.DeleteBuffer(handle);
    }
}
