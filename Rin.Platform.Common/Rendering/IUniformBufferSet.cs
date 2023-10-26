namespace Rin.Platform.Abstractions.Rendering;

public interface IUniformBufferSet {
    IUniformBuffer Get();
    IUniformBuffer Get_RT();
    IUniformBuffer Get(int frame);
    void Set(IUniformBuffer uniformBuffer, int frame);
}
