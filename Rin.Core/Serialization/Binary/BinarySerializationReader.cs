namespace Rin.Core.Serialization.Binary;

/// <summary>
///     Implements <see cref="SerializationStream" /> as a binary reader.
/// </summary>
public class BinarySerializationReader : SerializationStream {
    BinaryReader Reader { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="BinarySerializationReader" /> class.
    /// </summary>
    /// <param name="inputStream">The input stream to read from.</param>
    public BinarySerializationReader(Stream inputStream) {
        Reader = new(inputStream);
        UnderlyingStream = inputStream;
    }

    /// <inheritdoc />
    public override void Serialize(ref bool value) {
        value = Reader.ReadBoolean();
    }

    /// <inheritdoc />
    public override void Serialize(ref float value) {
        value = Reader.ReadSingle();
    }

    /// <inheritdoc />
    public override void Serialize(ref double value) {
        value = Reader.ReadDouble();
    }

    /// <inheritdoc />
    public override void Serialize(ref short value) {
        value = Reader.ReadInt16();
    }

    /// <inheritdoc />
    public override void Serialize(ref int value) {
        value = Reader.ReadInt32();
    }

    /// <inheritdoc />
    public override void Serialize(ref long value) {
        value = Reader.ReadInt64();
    }

    /// <inheritdoc />
    public override void Serialize(ref ushort value) {
        value = Reader.ReadUInt16();
    }

    /// <inheritdoc />
    public override void Serialize(ref uint value) {
        value = Reader.ReadUInt32();
    }

    /// <inheritdoc />
    public override void Serialize(ref ulong value) {
        value = Reader.ReadUInt64();
    }

    /// <inheritdoc />
    public override void Serialize(ref string value) {
        value = Reader.ReadString();
    }

    /// <inheritdoc />
    public override void Serialize(ref char value) {
        value = Reader.ReadChar();
    }

    /// <inheritdoc />
    public override void Serialize(ref byte value) {
        value = Reader.ReadByte();
    }

    /// <inheritdoc />
    public override void Serialize(ref sbyte value) {
        value = Reader.ReadSByte();
    }

    /// <inheritdoc />
    public override void Serialize(Span<byte> buffer) => Reader.Read(buffer);

    /// <inheritdoc />
    public override void Flush() { }
}
