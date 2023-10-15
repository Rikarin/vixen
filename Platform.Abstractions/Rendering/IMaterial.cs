using System.Numerics;

namespace Rin.Platform.Abstractions.Rendering; 

public interface IMaterial {
    string? Name { get; }
    IShader Shader { get; }
    MaterialFlags Flags { get; }
    
    void Set(string name, int value);
    void Set(string name, float value);
    void Set(string name, bool value);
    void Set(string name, Vector2 value);
    void Set(string name, Vector3 value);
    void Set(string name, Vector4 value);
    void Set(string name, Matrix4x4 value);
    void Set(string name, ITexture2D value);
    void Set(string name, ITexture2D value, int arrayIndex);
    void Set(string name, ITextureCube value);
    void Set(string name, IImageView value);

    int GetInt(string name);
    float GetFloat(string name);
    bool GetBool(string name);
    Vector2 GetVector2(string name);
    Vector3 GetVector3(string name);
    Vector4 GetVector4(string name);
    Matrix4x4 GetMatrix4x4(string name);
    ITexture2D GetTexture2D(string name);
    ITextureCube GetTextureCube(string name);

    // TODO


    void Prepare();
}