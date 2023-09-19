using Rin.Platform.Renderer;
using Silk.NET.OpenGL;

namespace Rin.Platform.Silk;

sealed class OpenGLVertexArray : Renderer.VertexArray {
    readonly List<VertexBuffer> vertexBuffers = new();
    readonly GL gl;
    readonly uint handle;
    uint vertexBufferIndex;

    public override IReadOnlyList<VertexBuffer> VertexBuffers => vertexBuffers.AsReadOnly();

    public OpenGLVertexArray() {
        gl = SilkWindow.MainWindow.Gl; // TODO: rewire this
        handle = gl.CreateVertexArray();
    }

    public override void Bind() {
        gl.BindVertexArray(handle);
    }

    public override void Unbind() {
        gl.BindVertexArray(0);
    }

    public override void Dispose() {
        base.Dispose();
        gl.DeleteVertexArray(handle);
    }

    public override unsafe void AddVertexBuffer(VertexBuffer buffer) {
        Bind();
        buffer.Bind();

        foreach (var element in buffer.Layout.Elements) {
            switch (element.Type) {
                case ShaderDataType.Float:
                case ShaderDataType.Float2:
                case ShaderDataType.Float3:
                case ShaderDataType.Float4: {
                    gl.EnableVertexAttribArray(vertexBufferIndex);
                    gl.VertexAttribPointer(
                        vertexBufferIndex,
                        element.ComponentCount,
                        ShaderDataTypeToOpenGLBaseType(element.Type),
                        element.Normalized,
                        buffer.Layout.Stride,
                        (void*)element.Offset
                    );
                    vertexBufferIndex++;
                    break;
                }
                case ShaderDataType.Int:
                case ShaderDataType.Int2:
                case ShaderDataType.Int3:
                case ShaderDataType.Int4:
                case ShaderDataType.Bool: {
                    gl.EnableVertexAttribArray(vertexBufferIndex);
                    gl.VertexAttribIPointer(
                        vertexBufferIndex,
                        element.ComponentCount,
                        ShaderDataTypeToOpenGLBaseType(element.Type),
                        buffer.Layout.Stride,
                        (void*)element.Offset
                    );
                    vertexBufferIndex++;
                    break;
                }
                case ShaderDataType.Mat3:
                case ShaderDataType.Mat4: {
                    for (var i = 0; i < element.ComponentCount; i++) {
                        gl.EnableVertexAttribArray(vertexBufferIndex);
                        gl.VertexAttribPointer(
                            vertexBufferIndex,
                            element.ComponentCount,
                            ShaderDataTypeToOpenGLBaseType(element.Type),
                            element.Normalized,
                            buffer.Layout.Stride,
                            (void*)(element.Offset + sizeof(float) * element.ComponentCount + i)
                        );
                        gl.VertexAttribDivisor(vertexBufferIndex, 1);
                        vertexBufferIndex++;
                    }

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        vertexBuffers.Add(buffer);
    }

    public override void SetIndexBuffer(IndexBuffer buffer) {
        Bind();
        buffer.Bind();
        IndexBuffer = buffer;
    }

    static GLEnum ShaderDataTypeToOpenGLBaseType(ShaderDataType type) =>
        type switch {
            ShaderDataType.Float => GLEnum.Float,
            ShaderDataType.Float2 => GLEnum.Float,
            ShaderDataType.Float3 => GLEnum.Float,
            ShaderDataType.Float4 => GLEnum.Float,
            ShaderDataType.Mat3 => GLEnum.Float,
            ShaderDataType.Mat4 => GLEnum.Float,
            ShaderDataType.Int => GLEnum.Int,
            ShaderDataType.Int2 => GLEnum.Int,
            ShaderDataType.Int3 => GLEnum.Int,
            ShaderDataType.Int4 => GLEnum.Int,
            ShaderDataType.Bool => GLEnum.Bool,
            _ => throw new ArgumentOutOfRangeException()
        };
}
