namespace Rin.Platform.Abstractions.Rendering;

public interface IStorageBufferSet {
    IStorageBuffer Get();
    IStorageBuffer Get_RT();
    IStorageBuffer Get(int frame);
    void Set(IStorageBuffer storageBuffer, int frame);
    void Resize(int newSize);
}
