namespace Vixen.Core.Storage;

/// <summary>
///     A read-only <see cref="NativeMemoryStream" /> that will properly keep references on its underlying
///     <see cref="Blob" />.
/// </summary>
class BlobStream : UnmanagedMemoryStream {
    readonly Blob blob;

    /// <inheritdoc />
    public override bool CanWrite => false;

    public unsafe BlobStream(Blob blob)
        : base((byte*)blob.Content, blob.Size, blob.Size, FileAccess.Read) {
        this.blob = blob;

        // Keep a reference on the blob while its data is used.
        this.blob.AddReference();
    }

    /// <inheritdoc />
    public override void WriteByte(byte value) => throw new NotSupportedException();

    /// <inheritdoc />
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    /// <inheritdoc />
    public override void Write(ReadOnlySpan<byte> buffer) => throw new NotSupportedException();

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        // Release reference on the blob
        blob.Release();
    }
}
