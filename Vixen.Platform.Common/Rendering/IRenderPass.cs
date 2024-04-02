namespace Vixen.Platform.Common.Rendering;

public interface IRenderPass {
    RenderPassOptions Options { get; }
    bool IsBaked { get; }
    IPipeline Pipeline { get; }
    IFramebuffer TargetFramebuffer { get; }
    int FirstSetIndex { get; }

    IImage2D GetOutput(int index);
    IImage GetDepthOutput();

    void SetInput(string name, IUniformBufferSet uniformBufferSet);
    void SetInput(string name, IUniformBuffer uniformBuffer);
    void SetInput(string name, IStorageBufferSet storageBufferSet);
    void SetInput(string name, IStorageBuffer storageBuffer);
    void SetInput(string name, ITexture2D texture);
    void SetInput(string name, ITextureCube textureCube);
    void SetInput(string name, IImage2D image);

    bool Validate();
    void Bake();
    void Prepare();
}