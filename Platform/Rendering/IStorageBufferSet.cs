namespace Rin.Platform.Rendering;

public interface IStorageBufferSet {
    public IStorageBuffer Get();
    public IStorageBuffer Get_RT();
    public IStorageBuffer Get(int frame);
    public void Set(IStorageBuffer storageBuffer, int frame);
    public void Resize(int newSize);
}
