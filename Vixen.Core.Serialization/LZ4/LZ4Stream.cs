using K4os.Compression.LZ4;
using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.CompilerServices;

namespace Vixen.Core.Serialization.LZ4;

/// <summary>Block compression stream. Allows to use LZ4 for stream compression.</summary>
public class LZ4Stream : Stream {
    /// <summary>The inner stream.</summary>
    readonly Stream innerStream;

    /// <summary>The compression mode.</summary>
    readonly CompressionMode compressionMode;

    /// <summary>The high compression flag (compression only).</summary>
    readonly bool highCompression;

    /// <summary>
    ///     Indicate whether the inner stream should be disposed or not.
    /// </summary>
    readonly bool disposeInnerStream;

    /// <summary>The block size (compression only).</summary>
    readonly int blockSize;

    /// <summary>The buffer.</summary>
    byte[] dataBuffer;

    /// <summary>The buffer containing the compressed read data</summary>
    byte[] compressedDataBuffer;

    /// <summary>The buffer length (can be different then _buffer.Length).</summary>
    int bufferLength;

    /// <summary>The offset in a buffer.</summary>
    int bufferOffset;

    /// <summary>
    ///     The position in the not compressed stream.
    /// </summary>
    long position;

    /// <summary>
    ///     The position in the inner stream.
    /// </summary>
    long innerStreamPosition;

    /// <summary>
    ///     The size of the stream after having been uncompressed.
    /// </summary>
    readonly long length;

    /// <summary>
    ///     The size of the compressed stream.
    /// </summary>
    readonly long compressedSize;

    /// <inheritdoc />
    public override bool CanRead => compressionMode == CompressionMode.Decompress;

    /// <inheritdoc />
    public override bool CanSeek => false;

    /// <inheritdoc />
    public override bool CanWrite => compressionMode == CompressionMode.Compress;

    /// <inheritdoc />
    public override long Length => length;

    /// <inheritdoc />
    public override long Position {
        get => position;
        set => throw NotSupported("SetPosition");
    }

    /// <summary>Initializes a new instance of the <see cref="LZ4Stream" /> class.</summary>
    /// <param name="innerStream">The inner stream.</param>
    /// <param name="compressionMode">The compression mode.</param>
    /// <param name="uncompressedSize">The size of the stream uncompressed</param>
    /// <param name="highCompression">if set to <c>true</c> [high compression].</param>
    /// <param name="disposeInnerStream">
    ///     if set to <c>true</c> <paramref name="innerStream" /> is disposed during called to
    ///     <see cref="Dispose" />
    /// </param>
    /// <param name="blockSize">Size of the block.</param>
    public LZ4Stream(
        Stream innerStream,
        CompressionMode compressionMode,
        bool highCompression = false,
        long uncompressedSize = -1,
        long compressedSize = -1,
        bool disposeInnerStream = false,
        int blockSize = 1024 * 1024
    ) {
        this.innerStream = innerStream;
        this.compressionMode = compressionMode;
        this.highCompression = highCompression;
        this.blockSize = System.Math.Max(16, blockSize);
        length = uncompressedSize;
        this.compressedSize = compressedSize;
        this.disposeInnerStream = disposeInnerStream;
    }

    /// <inheritdoc />
    public override void Flush() {
        if (bufferOffset > 0 && CanWrite) {
            FlushCurrentChunk();
        }
    }

    /// <inheritdoc />
    public override int ReadByte() {
        if (!CanRead) {
            throw NotSupported("Read");
        }

        if (bufferOffset >= bufferLength && !AcquireNextChunk()) {
            return -1; // that's just end of stream
        }

        position += 1;
        return dataBuffer[bufferOffset++];
    }

    /// <inheritdoc />
    public override unsafe int Read(byte[] buffer, int offset, int count) {
        if (!CanRead) {
            throw NotSupported("Read");
        }

        var total = 0;

        if (count > 0 && (buffer?.Length ?? 0) == 0) {
            throw new ArgumentOutOfRangeException(offset > 0 ? nameof(offset) : nameof(count));
        }

        while (count > 0) {
            var chunk = System.Math.Min(count, bufferLength - bufferOffset);
            if (chunk > 0) {
                // fixed yields null if array is empty or null
                fixed (byte* pSrc = dataBuffer)
                fixed (byte* pDst = buffer) {
                    Debug.Assert(pSrc is not null);
                    Unsafe.CopyBlockUnaligned(pDst + offset, pSrc + bufferOffset, (uint)chunk);
                }

                bufferOffset += chunk;
                offset += chunk;
                count -= chunk;
                total += chunk;
            } else {
                if (!AcquireNextChunk()) {
                    break;
                }
            }
        }

        position += total;
        return total;
    }

    /// <inheritdoc />
    public override int Read(Span<byte> buffer) {
        if (!CanRead) {
            throw NotSupported("Read");
        }

        var total = 0;

        while (buffer.Length > 0) {
            var chunk = System.Math.Min(buffer.Length, bufferLength - bufferOffset);
            if (chunk > 0) {
                var src = dataBuffer.AsSpan(bufferOffset, chunk);
                src.CopyTo(buffer);
                bufferOffset += chunk;
                buffer = buffer[chunk..];
                total += chunk;
            } else {
                if (!AcquireNextChunk()) {
                    break;
                }
            }
        }

        position += total;
        return total;
    }

    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin) {
        var newPosition = origin switch {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => Position + offset,
            SeekOrigin.End => throw NotSupported("Seek"),
            _ => throw new ArgumentOutOfRangeException(nameof(origin))
        };
        if (newPosition == 0) {
            innerStream.Seek(-innerStreamPosition, SeekOrigin.Current);
            Reset();
        } else if (newPosition == Position) {
            // nothing to do
        } else {
            throw NotSupported("Seek");
        }

        return Position;
    }

    /// <inheritdoc />
    public override void SetLength(long value) {
        throw NotSupported("SetLength");
    }

    /// <inheritdoc />
    public override void WriteByte(byte value) {
        if (!CanWrite) {
            throw NotSupported("Write");
        }

        position += 1;

        if (dataBuffer == null) {
            dataBuffer = new byte[blockSize];
            bufferLength = blockSize;
            bufferOffset = 0;
        }

        if (bufferOffset >= bufferLength) {
            FlushCurrentChunk();
        }

        dataBuffer[bufferOffset++] = value;
    }

    /// <inheritdoc />
    public override unsafe void Write(byte[] buffer, int offset, int count) {
        if (!CanWrite) {
            throw NotSupported("Write");
        }

        Debug.Assert(
            bufferLength >= 0
            && (dataBuffer is null || (uint)bufferOffset <= (uint)bufferLength)
            && (offset | count) >= 0
            && (uint)offset + (uint)count <= (uint)buffer.Length
        );

        position += count;

        if (dataBuffer == null) {
            dataBuffer = new byte[blockSize];
            bufferLength = blockSize;
            bufferOffset = 0;
        }

        while (count > 0) {
            var chunk = System.Math.Min(count, bufferLength - bufferOffset);
            if (chunk > 0) {
                fixed (byte* pSrc = buffer)
                fixed (byte* pDst = dataBuffer) {
                    Debug.Assert(pDst is not null);
                    Unsafe.CopyBlockUnaligned(pDst + bufferOffset, pSrc + offset, (uint)chunk);
                }

                offset += chunk;
                count -= chunk;
                bufferOffset += chunk;
            } else {
                FlushCurrentChunk();
            }
        }
    }

    /// <inheritdoc />
    public override void Write(ReadOnlySpan<byte> buffer) {
        if (!CanWrite) {
            throw NotSupported("Write");
        }

        Debug.Assert(bufferLength >= 0 && (dataBuffer is null || (uint)bufferOffset <= (uint)bufferLength));
        position += buffer.Length;

        if (dataBuffer == null) {
            dataBuffer = new byte[blockSize];
            bufferLength = blockSize;
            bufferOffset = 0;
        }

        while (buffer.Length > 0) {
            var chunk = System.Math.Min(buffer.Length, bufferLength - bufferOffset);
            if (chunk > 0) {
                var dst = dataBuffer.AsSpan(bufferOffset);
                buffer[..chunk].CopyTo(dst);
                buffer = buffer[chunk..];
                bufferOffset += chunk;
            } else {
                FlushCurrentChunk();
            }
        }
    }

    /// <summary>
    ///     Reset the stream to its initial position and state
    /// </summary>
    public void Reset() {
        position = 0;
        innerStreamPosition = 0;
        dataBuffer = null;
        bufferLength = 0;
        bufferOffset = 0;
    }

    /// <summary>Returns NotSupportedException.</summary>
    /// <param name="operationName">Name of the operation.</param>
    /// <returns>NotSupportedException</returns>
    static NotSupportedException NotSupported(string operationName) =>
        new($"Operation '{operationName}' is not supported");

    /// <summary>Tries to read variable length int.</summary>
    /// <param name="result">The result.</param>
    /// <returns>
    ///     <c>true</c> if integer has been read, <c>false</c> if end of stream has been
    ///     encountered at the start of a value.
    /// </returns>
    /// <exception cref="IOException">If end of stream has been encountered in the middle of a value.</exception>
    bool TryReadVarInt(out ulong result) {
        var buffer = new byte[1];
        var count = 0;
        result = 0;

        while (true) {
            if ((compressedSize != -1 && innerStreamPosition >= compressedSize)
                || innerStream.Read(buffer, 0, 1) == 0) {
                if (count == 0) {
                    return false;
                }

                throw new IOException("Unexpected end of stream");
            }

            innerStreamPosition += 1;
            var b = buffer[0];
            result += (ulong)(b & 0x7F) << count;
            count += 7;
            if ((b & 0x80) == 0 || count >= 64) {
                break;
            }
        }

        return true;
    }

    /// <summary>
    ///     Reads the variable length int. Work with assumption that value is in the stream
    ///     and throws exception if it isn't. If you want to check if value is in the stream
    ///     use <see cref="TryReadVarInt" /> instead.
    /// </summary>
    /// <returns>The value.</returns>
    /// <exception cref="IOException">The end of the stream was unexpectedly reached.</exception>
    ulong ReadVarInt() {
        if (!TryReadVarInt(out var result)) {
            throw new IOException("Unexpected end of stream");
        }

        return result;
    }

    /// <summary>
    ///     Reads the block of bytes.
    ///     Contrary to <see cref="Stream.Read" /> does not read partial data if possible.
    ///     If there is no data (yet) it waits.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="count">The length.</param>
    /// <returns>Number of bytes read.</returns>
    int ReadBlock(byte[] buffer, int offset, int count) {
        var total = 0;

        while (count > 0) {
            var read = innerStream.Read(buffer, offset, count);
            if (read == 0) {
                break;
            }

            innerStreamPosition += read;
            offset += read;
            count -= read;
            total += read;
        }

        return total;
    }

    /// <summary>Writes the variable length integer.</summary>
    /// <param name="value">The value.</param>
    void WriteVarInt(ulong value) {
        var buffer = new byte[1];
        while (true) {
            var b = (byte)(value & 0x7F);
            value >>= 7;
            buffer[0] = (byte)(b | (value == 0 ? 0 : 0x80));
            innerStream.Write(buffer, 0, 1);
            innerStreamPosition += 1;
            if (value == 0) {
                break;
            }
        }
    }

    /// <summary>Flushes current chunk.</summary>
    void FlushCurrentChunk() {
        if (bufferOffset <= 0) {
            return;
        }

        var compressed = new byte[bufferOffset];
        var level = highCompression ? LZ4Level.L09_HC : LZ4Level.L00_FAST;
        var compressedLength = LZ4Codec.Encode(dataBuffer, 0, bufferOffset, compressed, 0, bufferOffset, level);

        if (compressedLength <= 0 || compressedLength >= bufferOffset) {
            // uncompressible block
            compressed = dataBuffer;
            compressedLength = bufferOffset;
        }

        var isCompressed = compressedLength < bufferOffset;

        var flags = ChunkFlags.None;

        if (isCompressed) {
            flags |= ChunkFlags.Compressed;
        }

        if (highCompression) {
            flags |= ChunkFlags.HighCompression;
        }

        WriteVarInt((ulong)flags);
        WriteVarInt((ulong)bufferOffset);
        if (isCompressed) {
            WriteVarInt((ulong)compressedLength);
        }

        innerStream.Write(compressed, 0, compressedLength);
        innerStreamPosition += compressedLength;

        bufferOffset = 0;
    }

    /// <summary>Reads the next chunk from stream.</summary>
    /// <returns><c>true</c> if next has been read, or <c>false</c> if it is legitimate end of file.</returns>
    /// <exception cref="IOException">The end of the stream was unexpectedly reached.</exception>
    bool AcquireNextChunk() {
        do {
            if (!TryReadVarInt(out var varint)) {
                return false;
            }

            var flags = (ChunkFlags)varint;
            var isCompressed = (flags & ChunkFlags.Compressed) != 0;

            var originalLength = (int)ReadVarInt();
            var compressedLength = isCompressed ? (int)ReadVarInt() : originalLength;
            if (compressedLength > originalLength) {
                throw new IOException("Can't read beyond end of stream."); // corrupted
            }

            if (compressedDataBuffer == null || compressedDataBuffer.Length < compressedLength) {
                compressedDataBuffer = new byte[compressedLength];
            }

            var chunk = ReadBlock(compressedDataBuffer, 0, compressedLength);

            if (chunk != compressedLength) {
                throw new IOException("Can't read beyond end of stream."); // corrupted
            }

            if (!isCompressed) {
                // swap the buffers
                (compressedDataBuffer, dataBuffer) = (dataBuffer, compressedDataBuffer);
                bufferLength = compressedLength;
            } else {
                if (dataBuffer == null || dataBuffer.Length < originalLength) {
                    dataBuffer = new byte[originalLength];
                }

                var passes = (int)flags >> 2;
                if (passes != 0) {
                    throw new NotSupportedException("Chunks with multiple passes are not supported.");
                }

                LZ4Codec.Decode(compressedDataBuffer, 0, compressedLength, dataBuffer, 0, originalLength);
                bufferLength = originalLength;
            }

            bufferOffset = 0;
        } while (bufferLength == 0); // skip empty block (shouldn't happen but...)

        return true;
    }

    /// <summary>
    ///     Flags of a chunk. Please note, this
    /// </summary>
    [Flags]
    public enum ChunkFlags {
        /// <summary>None.</summary>
        None = 0x00,

        /// <summary>Set if chunk is compressed.</summary>
        Compressed = 0x01,

        /// <summary>
        ///     Set if high compression has been selected (does not affect decoder,
        ///     but might be useful when rewriting)
        /// </summary>
        HighCompression = 0x02,

        /// <summary>
        ///     3 bits for number of passes. Currently only 1 pass (value 0)
        ///     is supported.
        /// </summary>
        Passes = 0x04 | 0x08 | 0x10 // not used currently
    }

    /// <summary>
    ///     Releases the unmanaged resources used by the <see cref="T:System.IO.Stream" /> and optionally releases the
    ///     managed resources.
    /// </summary>
    /// <param name="disposing">
    ///     true to release both managed and unmanaged resources; false to release only unmanaged
    ///     resources.
    /// </param>
    protected override void Dispose(bool disposing) {
        Flush();
        if (disposeInnerStream) {
            innerStream.Dispose();
        }

        base.Dispose(disposing);
    }
}
