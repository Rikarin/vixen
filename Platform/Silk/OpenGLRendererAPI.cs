using Rin.Platform.Internal;
using Serilog;
using Silk.NET.OpenGL;
using System.Drawing;
using System.Runtime.InteropServices;
using VertexArray = Rin.Platform.Renderer.VertexArray;

namespace Rin.Platform.Silk;

class OpenGLRendererApi : IRendererApi {
    readonly GL gl;

    public OpenGLRendererApi() {
        gl = SilkWindow.MainWindow.Gl;
    }

    public unsafe void Initialize() {
        // TODO: if debug
        gl.Enable(EnableCap.DebugOutput);
        gl.Enable(EnableCap.DebugOutputSynchronous);
        // gl.DebugMessageCallback(Callback, IntPtr.Zero);
        // gl.DebugMessageControl(
        //     DebugSource.DontCare,
        //     DebugType.DontCare,
        //     DebugSeverity.DebugSeverityNotification,
        //     0,
        //     null,
        //     false
        // );
        // End if
        
        gl.Enable(EnableCap.Blend);
        gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        gl.Enable(EnableCap.DepthTest);
        gl.Enable(EnableCap.LineSmooth);
    }

    
    public void SetViewport(Point point, Size size) => gl.Viewport(point, size);
    public void SetClearColor(Color color) => gl.ClearColor(color);
    public void Clear() => gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

    public unsafe void Draw(VertexArray vertexArray, int? count) {
        vertexArray.Bind();
        var total = count ?? vertexArray.IndexBuffer.Count;
        gl.DrawElements(PrimitiveType.Triangles, (uint)total, DrawElementsType.UnsignedInt, null);
    }

    void Callback(GLEnum source, GLEnum type, int id, GLEnum severity, int length, IntPtr message, IntPtr userparam) {
        Log.Error("OpenGL Error - Severity: {Severity} Message {Error}", severity, Marshal.PtrToStringAuto(message));
    }
}
