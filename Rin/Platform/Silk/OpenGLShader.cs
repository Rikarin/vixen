using Editor.General;
using Silk.NET.OpenGL;
using System.Drawing;
using System.Numerics;
using Texture = Editor.General.Texture;

namespace Editor.Platform.Silk;

// TODO: Some of the methods may be reduced. eg. Material should convert Texture into buffer as well as offset and scale into Vector2
interface IInternalShader {
    void SetColor(string name, Color value);
    void SetColor(int id, Color value);
    void SetColorArray(string name, ReadOnlyMemory<Color> values);
    void SetColorArray(int id, ReadOnlyMemory<Color> values);
    void SetInteger(string name, int value);
    void SetInteger(int id, int value);
    void SetFloat(string name, float value);
    void SetFloat(int id, float value);
    void SetFloatArray(string name, ReadOnlyMemory<float> values);
    void SetFloatArray(int id, ReadOnlyMemory<float> values);
    void SetMatrix(string name, Matrix4x4 value);
    void SetMatrix(int id, Matrix4x4 value);
    void SetMatrixArray(string name, ReadOnlyMemory<Matrix4x4> values);
    void SetMatrixArray(int id, ReadOnlyMemory<Matrix4x4> values);
    void SetTexture(string name, Texture value);
    void SetTexture(int id, Texture value);
    void SetTexture(string name, RenderTexture value);
    void SetTexture(int id, RenderTexture value);
    void SetTextureOffset(string name, Vector2 value);
    void SetTextureOffset(int id, Vector2 value);
    void SetTextureScale(string name, Vector2 value);
    void SetTextureScale(int id, Vector2 value);
    void SetVector(string name, Vector4 value);
    void SetVector(int id, Vector4 value);
    void SetVectorArray(string name, ReadOnlyMemory<Vector4> values);
    void SetVectorArray(int id, ReadOnlyMemory<Vector4> values);
}


class OpenGLShader {
    GL gl;
    uint handle;
    
    public OpenGLShader() {
        gl = SilkWindow.MainWindow.Gl;
    }

    // TODO: return size and type as well so the information can be used in the editor later
    public IEnumerable<string> GetUniforms() {
        var count = gl.GetProgram(handle, GLEnum.ActiveUniforms);
        
        for (uint i = 0; i < count; i++) {
            var name = gl.GetActiveUniform(handle, i, out var size, out var type);
            if (name != null) {
                yield return name;
            }
        }
    }
}
