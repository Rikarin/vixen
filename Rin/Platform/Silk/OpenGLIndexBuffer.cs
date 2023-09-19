using Editor.Renderer;
using Silk.NET.OpenGL;

namespace Editor.Platform.Silk;

public class OpenGLIndexBuffer : IndexBuffer {
    readonly GL gl;
    readonly uint handle;

    public override int Count { get; }

    public OpenGLIndexBuffer(ReadOnlySpan<uint> indices) {
        gl = SilkWindow.MainWindow.Gl; // TODO: rewire this
        Count = indices.Length;
        handle = gl.CreateBuffer();

        gl.BindBuffer(GLEnum.ArrayBuffer, handle);
        gl.BufferData(GLEnum.ArrayBuffer, indices, GLEnum.StaticDraw);

        // unsafe {
        //     fixed (void* ptr = indices) {
        //         gl.BufferData(GLEnum.ArrayBuffer, (UIntPtr)(indices.Length * sizeof(uint)), ptr, GLEnum.StaticDraw);
        //     }
        // }
    }

    public override void Bind() {
        gl.BindBuffer(GLEnum.ElementArrayBuffer, handle);
    }

    public override void Unbind() {
        gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
    }

    public override void Dispose() {
        base.Dispose();
        gl.DeleteBuffer(handle);
    }
}
