using Rin.Platform.Internal;

namespace Rin.Core.General;

public abstract class Texture {
    // TODO: should this be here? If yes, then rename it
    internal IInternalTexture2D handle;
    public int Width { get; protected set; }

    public int Height { get; protected set; }
    // public TextureDimension Dimension { get; }
    // Wrap modes

    /// <summary>
    ///     True when copy of the data is stored in CPU memory
    ///     Set to true when GetPixel()/SetPixel() needs to be called
    /// </summary>
    public virtual bool IsReadable { get; set; }
}

public class RenderTexture : Texture {
    // TODO
}

public class Texture2D : Texture {
    bool isReadable;
    byte[]? buffer;

    public TextureFormat Format { get; }

    public override bool IsReadable {
        get => isReadable;
        set {
            isReadable = value;
            buffer = value ? new byte[Width * Height * GetTextureFormatSize(Format)] : null;
        }
    }

    // mip, linear
    public Texture2D(int width, int height, TextureFormat textureFormat = TextureFormat.RGBA32) {
        // handle = new OpenGLTexture2D((uint)width, (uint)height, textureFormat); // TODO
        Width = width;
        Height = height;
        Format = textureFormat;
    }

    public Span<byte> GetRawTextureData() {
        EnsureReadable();
        return buffer;
    }

    public void LoadRawTextureData(ReadOnlySpan<byte> data) {
        EnsureReadable();
    }

    public void Apply(bool updateMipmaps = true, bool makeNoLongerReadable = false) {
        EnsureReadable();
        // TODO: mips
        handle.SetData<byte>(buffer);
    }

    void EnsureReadable() {
        if (!IsReadable) {
            throw new TextureNotReadableException();
        }
    }

    static int GetTextureFormatSize(TextureFormat format) =>
        // TODO: verify these sizes
        format switch {
            TextureFormat.R8 => 1,
            TextureFormat.RGB8 => 1,
            TextureFormat.RGBA8 => 1,
            TextureFormat.RGBA32 => 4,
            _ => throw new ArgumentOutOfRangeException()
        };
}

public class TextureNotReadableException : Exception { }
