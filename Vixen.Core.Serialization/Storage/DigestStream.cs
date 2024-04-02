namespace Vixen.Core.Storage;

public class DigestStream : OdbStreamWriter {
    ObjectIdBuilder builder;

    public override ObjectId CurrentHash => builder.ComputeHash();

    public DigestStream(Stream stream) : base(stream, null) { }

    internal DigestStream(Stream stream, string temporaryName) : base(stream, temporaryName) { }

    public void Reset() {
        Position = 0;
        builder.Reset();
    }

    public override void WriteByte(byte value) {
        builder.WriteByte(value);
        stream.WriteByte(value);
    }

    public override void Write(byte[] buffer, int offset, int count) {
        builder.Write(buffer, offset, count);
        stream.Write(buffer, offset, count);
    }

    public override void Write(ReadOnlySpan<byte> buffer) {
        builder.Write(buffer);
        stream.Write(buffer);
    }
}
