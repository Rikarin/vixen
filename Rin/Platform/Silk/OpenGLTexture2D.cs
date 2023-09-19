using Editor.General;
using Editor.Platform.Internal;
using Silk.NET.OpenGL;

namespace Editor.Platform.Silk;

sealed class OpenGLTexture2D : IInternalTexture2D, IDisposable {
    readonly GL gl;
    readonly PixelFormat dataFormat;
    readonly SizedInternalFormat internalFormat;
    readonly uint width;
    readonly uint height;
    readonly uint handle;

    internal OpenGLTexture2D(uint width, uint height, TextureFormat format) {
        gl = SilkWindow.MainWindow.Gl;
        this.width = width;
        this.height = height;
        internalFormat = format.ToInternalFormat();
        dataFormat = format.ToDataFormat();

        handle = gl.CreateTexture(TextureTarget.Texture2D);
        gl.TextureStorage2D(handle, 0, internalFormat, width, height);

        gl.TextureParameterI(handle, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
        gl.TextureParameterI(handle, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);

        // TODO: set this according to TextureWrapMode
        gl.TextureParameterI(handle, TextureParameterName.TextureWrapS, (int)GLEnum.Repeat);
        gl.TextureParameterI(handle, TextureParameterName.TextureWrapT, (int)GLEnum.Repeat);
    }

    public void SetData<T>(ReadOnlySpan<T> data) where T : unmanaged {
        gl.TextureSubImage2D(handle, 0, 0, 0, width, height, dataFormat, PixelType.Byte, data);

        // TODO: not sure about this
        // gl.GenerateMipmap(TextureTarget.Texture2D);
    }

    public void Bind(uint unit) {
        // When BindTextureUnit won't work as we would want to support older platforms as well
        // https://stackoverflow.com/questions/60513272/alternative-for-glbindtextureunit-for-opengl-versions-less-than-4-5
        gl.BindTextureUnit(unit, handle);
    }

    public void Dispose() {
        gl.DeleteTexture(handle);
    }
}

static class TextureFormatExtensions {
    internal static PixelFormat ToDataFormat(this TextureFormat format) =>
        format switch {
            TextureFormat.RGB8 => PixelFormat.Rgb,
            TextureFormat.RGBA8 => PixelFormat.Rgba,
            _ => throw new ArgumentOutOfRangeException()
        };

    internal static SizedInternalFormat ToInternalFormat(this TextureFormat format) =>
        format switch {
            TextureFormat.RGB8 => SizedInternalFormat.Rgb8,
            TextureFormat.RGBA8 => SizedInternalFormat.Rgba8,
            _ => throw new ArgumentOutOfRangeException()
        };
}
