using System.Drawing;

namespace Vixen.Core.Common;

// TODO: options pattern
public sealed class ImageOptions {
    public string DebugName { get; set; }
    public ImageFormat Format { get; set; } = ImageFormat.Rgba;
    public ImageUsage Usage { get; set; } = ImageUsage.Texture;
    public bool Transfer { get; set; }
    public Size Size { get; set; } = new(1, 1);
    public int Mips { get; set; } = 1;
    public int Layers { get; set; } = 1;
    public bool CreateSampler { get; set; } = true;
}
