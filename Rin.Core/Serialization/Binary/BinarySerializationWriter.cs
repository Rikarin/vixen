namespace Rin.Core.Serialization.Binary;

/// <summary>
///     Implements <see cref="SerializationStream" /> as a binary writer.
/// </summary>
public class BinarySerializationWriter : SerializationStream {
    BinaryWriter Writer { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="BinarySerializationWriter" /> class.
    /// </summary>
    /// <param name="outputStream">The output stream.</param>
    public BinarySerializationWriter(Stream outputStream) {
        Writer = new(outputStream);
        UnderlyingStream = outputStream;
    }

    /// <inheritdoc />
    public override void Serialize(ref bool value) {
        Writer.Write(value);
    }

    /// <inheritdoc />
    public override void Serialize(ref float value) {
        Writer.Write(value);
    }

    /// <inheritdoc />
    public override void Serialize(ref double value) {
        Writer.Write(value);
    }

    /// <inheritdoc />
    public override void Serialize(ref short value) {
        Writer.Write(value);
    }

    /// <inheritdoc />
    public override void Serialize(ref int value) {
        Writer.Write(value);
    }

    /// <inheritdoc />
    public override void Serialize(ref long value) {
        Writer.Write(value);
    }

    /// <inheritdoc />
    public override void Serialize(ref ushort value) {
        Writer.Write(value);
    }

    /// <inheritdoc />
    public override void Serialize(ref uint value) {
        Writer.Write(value);
    }

    /// <inheritdoc />
    public override void Serialize(ref ulong value) {
        Writer.Write(value);
    }

    /// <inheritdoc />
    public override void Serialize(ref string value) {
        Writer.Write(value);
    }

    /// <inheritdoc />
    public override void Serialize(ref char value) {
        Writer.Write(value);
    }

    /// <inheritdoc />
    public override void Serialize(ref byte value) {
        Writer.Write(value);
    }

    /// <inheritdoc />
    public override void Serialize(ref sbyte value) {
        Writer.Write(value);
    }

    /// <inheritdoc />
    public override void Serialize(Span<byte> buffer) => Writer.Write(buffer);

    /// <inheritdoc />
    public override void Flush() {
        Writer.Flush();
    }
}
