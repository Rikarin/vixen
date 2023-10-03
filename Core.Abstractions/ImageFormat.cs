namespace Rin.Core.Abstractions;

public enum ImageFormat {
    None,
    Red8Un,
    Red8Ui,
    Red16Ui,
    Red32Ui,
    Red32F,
    Rg8,
    Rg16F,
    Rg32F,
    Rgb,
    Rgba,
    Rgba16F,
    Rgba32F,

    B10R11G11Uf,

    Srgb,

    Depth32FStencil8Uint,
    Depth32F,
    Depth24Stencil8,

    // Defaults
    Depth = Depth24Stencil8
}