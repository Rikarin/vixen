using System.Drawing;
using System.Numerics;

namespace Rin.Platform.Internal;

// TODO: Some of the methods may be reduced. eg. Material should convert Texture into buffer as well as offset and scale into Vector2
// TODO: Texture should be as is because OpenGL needs to decide on location binding??
interface IInternalShader {
    void Bind();
    int PropertyToId(string name);
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
    void SetTexture(string name, IInternalTexture2D value);
    void SetTexture(int id, IInternalTexture2D value);
    void SetTextureOffset(string name, Vector2 value);
    void SetTextureOffset(int id, Vector2 value);
    void SetTextureScale(string name, Vector2 value);
    void SetTextureScale(int id, Vector2 value);
    void SetVector(string name, Vector4 value);
    void SetVector(int id, Vector4 value);
    void SetVectorArray(string name, ReadOnlyMemory<Vector4> values);
    void SetVectorArray(int id, ReadOnlyMemory<Vector4> values);
}
