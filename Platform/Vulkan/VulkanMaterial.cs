using Rin.Platform.Abstractions.Rendering;
using System.Numerics;

namespace Rin.Platform.Vulkan;

sealed class VulkanMaterial : IMaterial {
    DescriptorSetManager descriptorSetManager;

    // TODO: not sure about this
    readonly Memory<byte> bufferTest = new byte[42];

    public string? Name { get; }
    public IShader Shader { get; }
    public MaterialFlags Flags { get; private set; }
    public ReadOnlyMemory<byte> UniformStorageBuffer => bufferTest;

    public VulkanMaterial(IShader shader, string? name) {
        Shader = shader;
        Name = name;

        Initialize();
    }

    public void Set(string name, int value) {
        throw new NotImplementedException();
    }

    public void Set(string name, float value) => InternalSet(name, value);
    public void Set(string name, bool value) => InternalSet(name, value);
    public void Set(string name, Vector2 value) => InternalSet(name, value);
    public void Set(string name, Vector3 value) => InternalSet(name, value);
    public void Set(string name, Vector4 value) => InternalSet(name, value);
    public void Set(string name, Matrix4x4 value) => InternalSet(name, value);
    public void Set(string name, ITexture2D value) => descriptorSetManager.SetInput(name, value);

    public void Set(string name, ITexture2D value, int arrayIndex) =>
        descriptorSetManager.SetInput(name, value, arrayIndex);

    public void Set(string name, ITextureCube value) => descriptorSetManager.SetInput(name, value);
    public void Set(string name, IImageView value) => descriptorSetManager.SetInput(name, value);

    public int GetInt(string name) => InternalGet<int>(name);
    public float GetFloat(string name) => InternalGet<float>(name);
    public bool GetBool(string name) => InternalGet<bool>(name);
    public Vector2 GetVector2(string name) => InternalGet<Vector2>(name);
    public Vector3 GetVector3(string name) => InternalGet<Vector3>(name);
    public Vector4 GetVector4(string name) => InternalGet<Vector4>(name);
    public Matrix4x4 GetMatrix4x4(string name) => InternalGet<Matrix4x4>(name);
    public ITexture2D GetTexture2D(string name) => GetResource<ITexture2D>(name);
    public ITextureCube GetTextureCube(string name) => GetResource<ITextureCube>(name);

    public void Prepare() => descriptorSetManager.InvalidateAndUpdate();

    void InternalSet<T>(string name, T value) { }

    T InternalGet<T>(string name) => throw new NotImplementedException();

    T GetResource<T>(string name) => throw new NotImplementedException();

    void Initialize() {
        Flags |= MaterialFlags.DepthText | MaterialFlags.Blend;

        descriptorSetManager = new(
            new() {
                DebugName = Name ?? $"{Shader.Name} (Material)",
                Shader = Shader as VulkanShader,
                DefaultResources = true,
                StartSet = 0,
                EndSet = 0
            }
        );
        
        // TODO
        
        descriptorSetManager.Bake();
    }
}
