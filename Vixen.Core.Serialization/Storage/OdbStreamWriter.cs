using Vixen.Core.Storage;

namespace Vixen.Core.Serialization.Storage;

public abstract class OdbStreamWriter : Stream {
    public Action<OdbStreamWriter> Disposed;

    public string? TemporaryName;
    protected readonly Stream stream;
    readonly long initialPosition;

    public abstract ObjectId CurrentHash { get; }
    public override bool CanRead => false;
    public override bool CanSeek => true;
    public override bool CanWrite => stream.CanWrite;
    public override long Length => stream.Length - initialPosition;

    public override long Position {
        get => stream.Position - initialPosition;
        set => stream.Position = initialPosition + value;
    }

    protected OdbStreamWriter(Stream stream, string? temporaryName) {
        this.stream = stream;
        initialPosition = stream.Position;
        TemporaryName = temporaryName;
    }

    public override void Flush() => stream.Flush();
    public override int Read(byte[] buffer, int offset, int count) => throw new InvalidOperationException();
    public override long Seek(long offset, SeekOrigin origin) => stream.Seek(offset, origin);
    public override void SetLength(long value) => throw new InvalidOperationException();

    protected override void Dispose(bool disposing) {
        // Force hash computation before stream is closed.
        var hash = CurrentHash;
        stream.Dispose();

        Disposed?.Invoke(this);
    }
}
