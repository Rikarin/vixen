namespace Rin.Platform.Rendering;

public interface IUniformBufferSet {
    public IUniformBuffer Get();
    public IUniformBuffer Get_RT();
    public IUniformBuffer Get(int frame);
    public void Set(IUniformBuffer uniformBuffer, int frame);
}
