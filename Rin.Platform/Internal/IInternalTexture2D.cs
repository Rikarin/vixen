namespace Rin.Platform.Internal;

interface IInternalTexture2D {
    void SetData<T>(ReadOnlySpan<T> data) where T : unmanaged;
    void Bind(uint unit);
}
