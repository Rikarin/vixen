namespace Vixen.Core.Common;

public static class ImageFormatExtensions {
    public static bool IsDepthFormat(this ImageFormat format) =>
        format is ImageFormat.Depth24Stencil8 or ImageFormat.Depth32F or ImageFormat.Depth32FStencil8Uint;
}
