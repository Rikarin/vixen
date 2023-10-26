using System.Drawing;

namespace Rin.Platform.Abstractions.Rendering;

public sealed class RenderPassOptions {
    public IPipeline Pipeline { get; set; }
    public string DebugName { get; set; }
    public Color MarkerColor { get; set; }
}
