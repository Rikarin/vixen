namespace Rin.Platform.Rendering;

public interface IRenderPass {
    public RenderPassOptions Options { get; }
    public bool IsBaked { get; }
    public IPipeline Pipeline { get; }
    public IFramebuffer TargetFramebuffer { get; }
    public int FirstSetIndex { get; }

    void SetIndex(string name, IUniformBufferSet uniformBufferSet);
    void SetIndex(string name, IUniformBuffer uniformBuffer);
    
    void SetIndex(string name, IStorageBufferSet storageBufferSet);
    void SetIndex(string name, IStorageBuffer storageBuffer);
    
    // void SetIndex(string name, IUniformBufferSet uniformBufferSet);
    // void SetIndex(string name, IUniformBufferSet uniformBufferSet);
    // void SetIndex(string name, IUniformBufferSet uniformBufferSet);
    // virtual void SetInput(std::string_view name, Ref<Texture2D> texture) = 0;
    // virtual void SetInput(std::string_view name, Ref<TextureCube> textureCube) = 0;
    // virtual void SetInput(std::string_view name, Ref<Image2D> image) = 0;

    IImage2D GetOutput(int index);
    IImage GetDepthOutput();

    bool Validate();
    void Bake();
    void Prepare();
}