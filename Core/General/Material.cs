using System.Drawing;
using System.Numerics;

namespace Rin.Core.General;

// TODO: remove id when name is set; otherwise as well ??
// TODO: SetBuffer, SetConstantBuffer, Keyword, OverrideTag, Pass, PropertyLock, PassEnabled

public class Material {
    readonly Dictionary<int, MaterialValue> idBuffer = new();
    readonly Dictionary<string, MaterialValue> nameBuffer = new();

    public Shader Shader { get; }

    // TODO: Not sure if MainTexture, MainTextureScale and MainTextureOffset should be nullable or not

    public Texture MainTexture {
        get => GetTexture("_MainTex");
        set => SetTexture("_MainText", value);
    }

    public Vector2 MainTextureOffset {
        get => GetTextureOffset("_MainTexOffset");
        set => SetTextureOffset("_MainTexOffset", value);
    }

    public Vector2 MainTextureScale {
        get => GetTextureOffset("_MainTexScale");
        set => SetTextureOffset("_MainTexScale", value);
    }

    public Material(Material original) {
        throw new NotImplementedException();
    }

    public Material(Shader shader) {
        Shader = shader;
    }

    public bool HasColor(string name) => HasType(MaterialType.Color, name);
    public bool HasColor(int id) => HasType(MaterialType.Color, id);
    public bool HasColorArray(string name) => HasType(MaterialType.ColorArray, name);
    public bool HasColorArray(int id) => HasType(MaterialType.ColorArray, id);
    public bool HasInteger(string name) => HasType(MaterialType.Integer, name);
    public bool HasInteger(int id) => HasType(MaterialType.Integer, id);
    public bool HasFloat(string name) => HasType(MaterialType.Float, name);
    public bool HasFloat(int id) => HasType(MaterialType.Float, id);
    public bool HasFloatArray(string name) => HasType(MaterialType.FloatArray, name);
    public bool HasFloatArray(int id) => HasType(MaterialType.FloatArray, id);
    public bool HasMatrix(string name) => HasType(MaterialType.Matrix, name);
    public bool HasMatrix(int id) => HasType(MaterialType.Matrix, id);
    public bool HasMatrixArray(string name) => HasType(MaterialType.MatrixArray, name);
    public bool HasMatrixArray(int id) => HasType(MaterialType.MatrixArray, id);

    public bool HasTexture(string name) =>
        HasType(MaterialType.Texture, name) || HasType(MaterialType.RenderTexture, name);

    public bool HasTexture(int id) => HasType(MaterialType.Texture, id) || HasType(MaterialType.RenderTexture, id);
    public bool HasTextureOffset(string name) => HasType(MaterialType.TextureOffset, name);
    public bool HasTextureOffset(int id) => HasType(MaterialType.TextureOffset, id);
    public bool HasTextureScale(string name) => HasType(MaterialType.TextureScale, name);
    public bool HasTextureScale(int id) => HasType(MaterialType.TextureScale, id);
    public bool HasVector(string name) => HasType(MaterialType.Vector, name);
    public bool HasVector(int id) => HasType(MaterialType.Vector, id);
    public bool HasVectorArray(string name) => HasType(MaterialType.VectorArray, name);
    public bool HasVectorArray(int id) => HasType(MaterialType.VectorArray, id);

    public Color GetColor(string name) {
        if (!HasColor(name)) {
            throw new MaterialValueNotSetException(name);
        }

        return (Color)nameBuffer[name].Value;
    }

    public Color GetColor(int id) {
        if (!HasColor(id)) {
            throw new MaterialValueNotSetException(id);
        }

        return (Color)idBuffer[id].Value;
    }

    public ReadOnlyMemory<Color> GetColorArray(string name) {
        if (!HasColorArray(name)) {
            throw new MaterialValueNotSetException(name);
        }

        return (ReadOnlyMemory<Color>)nameBuffer[name].Value;
    }

    public ReadOnlyMemory<Color> GetColorArray(int id) {
        if (!HasColorArray(id)) {
            throw new MaterialValueNotSetException(id);
        }

        return (ReadOnlyMemory<Color>)idBuffer[id].Value;
    }

    public int GetInteger(string name) {
        if (!HasInteger(name)) {
            throw new MaterialValueNotSetException(name);
        }

        return (int)nameBuffer[name].Value;
    }

    public int GetInteger(int id) {
        if (!HasInteger(id)) {
            throw new MaterialValueNotSetException(id);
        }

        return (int)idBuffer[id].Value;
    }

    public float GetFloat(string name) {
        if (!HasFloat(name)) {
            throw new MaterialValueNotSetException(name);
        }

        return (float)nameBuffer[name].Value;
    }

    public float GetFloat(int id) {
        if (!HasFloat(id)) {
            throw new MaterialValueNotSetException(id);
        }

        return (float)idBuffer[id].Value;
    }

    public ReadOnlyMemory<float> GetFloatArray(string name) {
        if (!HasFloatArray(name)) {
            throw new MaterialValueNotSetException(name);
        }

        return (ReadOnlyMemory<float>)nameBuffer[name].Value;
    }

    public ReadOnlyMemory<float> GetFloatArray(int id) {
        if (!HasFloatArray(id)) {
            throw new MaterialValueNotSetException(id);
        }

        return (ReadOnlyMemory<float>)idBuffer[id].Value;
    }

    public Matrix4x4 GetMatrix(string name) {
        if (!HasMatrix(name)) {
            throw new MaterialValueNotSetException(name);
        }

        return (Matrix4x4)nameBuffer[name].Value;
    }

    public Matrix4x4 GetMatrix(int id) {
        if (!HasMatrix(id)) {
            throw new MaterialValueNotSetException(id);
        }

        return (Matrix4x4)idBuffer[id].Value;
    }

    public ReadOnlyMemory<Matrix4x4> GetMatrixArray(string name) {
        if (!HasMatrixArray(name)) {
            throw new MaterialValueNotSetException(name);
        }

        return (ReadOnlyMemory<Matrix4x4>)nameBuffer[name].Value;
    }

    public ReadOnlyMemory<Matrix4x4> GetMatrixArray(int id) {
        if (!HasMatrixArray(id)) {
            throw new MaterialValueNotSetException(id);
        }

        return (ReadOnlyMemory<Matrix4x4>)idBuffer[id].Value;
    }

    public Texture GetTexture(string name) {
        if (!HasTexture(name)) {
            throw new MaterialValueNotSetException(name);
        }

        return (Texture)nameBuffer[name].Value;
    }

    public Texture GetTexture(int id) {
        if (!HasTexture(id)) {
            throw new MaterialValueNotSetException(id);
        }

        return (Texture)idBuffer[id].Value;
    }

    public Vector2 GetTextureOffset(string name) {
        if (!HasTextureOffset(name)) {
            throw new MaterialValueNotSetException(name);
        }

        return (Vector2)nameBuffer[name].Value;
    }

    public Vector2 GetTextureOffset(int id) {
        if (!HasTextureOffset(id)) {
            throw new MaterialValueNotSetException(id);
        }

        return (Vector2)idBuffer[id].Value;
    }

    public Vector2 GetTextureScale(string name) {
        if (!HasTextureScale(name)) {
            throw new MaterialValueNotSetException(name);
        }

        return (Vector2)nameBuffer[name].Value;
    }

    public Vector2 GetTextureScale(int id) {
        if (!HasTextureScale(id)) {
            throw new MaterialValueNotSetException(id);
        }

        return (Vector2)idBuffer[id].Value;
    }

    public Vector4 GetVector(string name) {
        if (!HasVector(name)) {
            throw new MaterialValueNotSetException(name);
        }

        return (Vector4)nameBuffer[name].Value;
    }

    public Vector4 GetVector(int id) {
        if (!HasVector(id)) {
            throw new MaterialValueNotSetException(id);
        }

        return (Vector4)idBuffer[id].Value;
    }

    public ReadOnlyMemory<Vector4> GetVectorArray(string name) {
        if (!HasVectorArray(name)) {
            throw new MaterialValueNotSetException(name);
        }

        return (ReadOnlyMemory<Vector4>)nameBuffer[name].Value;
    }

    public ReadOnlyMemory<Vector4> GetVectorArray(int id) {
        if (!HasVectorArray(id)) {
            throw new MaterialValueNotSetException(id);
        }

        return (ReadOnlyMemory<Vector4>)idBuffer[id].Value;
    }

    public void SetColor(string name, Color value) {
        // nameBuffer[name] = new(MaterialType.Color, value, () => Shader.Handle.SetColor(name, value));
    }

    public void SetColor(int id, Color value) {
        // idBuffer[id] = new(MaterialType.Color, value, () => Shader.Handle.SetColor(id, value));
    }

    public void SetColorArray(string name, ReadOnlyMemory<Color> values) {
        // nameBuffer[name] = new(MaterialType.ColorArray, values, () => Shader.Handle.SetColorArray(name, values));
    }

    public void SetColorArray(int id, ReadOnlyMemory<Color> values) {
        // idBuffer[id] = new(MaterialType.ColorArray, values, () => Shader.Handle.SetColorArray(id, values));
    }

    public void SetInteger(string name, int value) {
        // nameBuffer[name] = new(MaterialType.Integer, value, () => Shader.Handle.SetInteger(name, value));
    }

    public void SetInteger(int id, int value) {
        // idBuffer[id] = new(MaterialType.Integer, value, () => Shader.Handle.SetInteger(id, value));
    }

    public void SetFloat(string name, float value) {
        // nameBuffer[name] = new(MaterialType.Float, value, () => Shader.Handle.SetFloat(name, value));
    }

    public void SetFloat(int id, float value) {
        // idBuffer[id] = new(MaterialType.Float, value, () => Shader.Handle.SetFloat(id, value));
    }

    public void SetFloatArray(string name, ReadOnlyMemory<float> values) {
        // nameBuffer[name] = new(MaterialType.FloatArray, values, () => Shader.Handle.SetFloatArray(name, values));
    }

    public void SetFloatArray(int id, ReadOnlyMemory<float> values) {
        // idBuffer[id] = new(MaterialType.FloatArray, values, () => Shader.Handle.SetFloatArray(id, values));
    }

    public void SetMatrix(string name, Matrix4x4 value) {
        // nameBuffer[name] = new(MaterialType.Matrix, value, () => Shader.Handle.SetMatrix(name, value));
    }

    public void SetMatrix(int id, Matrix4x4 value) {
        // idBuffer[id] = new(MaterialType.Matrix, value, () => Shader.Handle.SetMatrix(id, value));
    }

    public void SetMatrixArray(string name, ReadOnlyMemory<Matrix4x4> values) {
        // nameBuffer[name] = new(MaterialType.MatrixArray, values, () => Shader.Handle.SetMatrixArray(name, values));
    }

    public void SetMatrixArray(int id, ReadOnlyMemory<Matrix4x4> values) {
        // idBuffer[id] = new(MaterialType.MatrixArray, values, () => Shader.Handle.SetMatrixArray(id, values));
    }

    public void SetTexture(string name, Texture value) {
        // nameBuffer[name] = new(MaterialType.Texture, value, () => Shader.Handle.SetTexture(name, value.handle));
    }

    public void SetTexture(int id, Texture value) {
        // idBuffer[id] = new(MaterialType.Texture, value, () => Shader.Handle.SetTexture(id, value.handle));
    }

    public void SetTexture(string name, RenderTexture value) {
        // nameBuffer[name] = new(MaterialType.RenderTexture, value, () => Shader.Handle.SetTexture(name, value.handle));
    }

    public void SetTexture(int id, RenderTexture value) {
        // idBuffer[id] = new(MaterialType.RenderTexture, value, () => Shader.Handle.SetTexture(id, value.handle));
    }

    public void SetTextureOffset(string name, Vector2 value) {
        // nameBuffer[name] = new(MaterialType.TextureOffset, value, () => Shader.Handle.SetTextureOffset(name, value));
    }

    public void SetTextureOffset(int id, Vector2 value) {
        // idBuffer[id] = new(MaterialType.TextureOffset, value, () => Shader.Handle.SetTextureOffset(id, value));
    }

    public void SetTextureScale(string name, Vector2 value) {
        // nameBuffer[name] = new(MaterialType.TextureScale, value, () => Shader.Handle.SetTextureScale(name, value));
    }

    public void SetTextureScale(int id, Vector2 value) {
        // idBuffer[id] = new(MaterialType.TextureScale, value, () => Shader.Handle.SetTextureScale(id, value));
    }

    public void SetVector(string name, Vector4 value) {
        // nameBuffer[name] = new(MaterialType.Vector, value, () => Shader.Handle.SetVector(name, value));
    }

    public void SetVector(int id, Vector4 value) {
        // idBuffer[id] = new(MaterialType.Vector, value, () => Shader.Handle.SetVector(id, value));
    }

    public void SetVectorArray(string name, ReadOnlyMemory<Vector4> values) {
        // nameBuffer[name] = new(MaterialType.VectorArray, values, () => Shader.Handle.SetVectorArray(name, values));
    }

    public void SetVectorArray(int id, ReadOnlyMemory<Vector4> values) {
        // idBuffer[id] = new(MaterialType.VectorArray, values, () => Shader.Handle.SetVectorArray(id, values));
    }

    bool HasType(MaterialType type, int id) {
        if (idBuffer.TryGetValue(id, out var value)) {
            return value.Type == type;
        }

        return false;
    }

    bool HasType(MaterialType type, string name) {
        if (nameBuffer.TryGetValue(name, out var value)) {
            return value.Type == type;
        }

        return false;
    }

    // internal
    public void Render() {
        // Shader.Handle.Bind();
        // Shader.Render();
        // TODO: Bind shader and then render??

        foreach (var x in nameBuffer.Values) {
            x.Callback();
        }

        foreach (var x in idBuffer.Values) {
            x.Callback();
        }
    }

    record struct MaterialValue(MaterialType Type, object Value, Action Callback);

    enum MaterialType {
        Color,
        ColorArray,
        Integer,
        Float,
        FloatArray,
        Matrix,
        MatrixArray,
        Texture,
        RenderTexture,
        TextureOffset,
        TextureScale,
        Vector,
        VectorArray
    }
}

public class MaterialValueNotSetException : Exception {
    public MaterialValueNotSetException(int id) : base($"Id {id}") { }
    public MaterialValueNotSetException(string name) : base(name) { }
}
