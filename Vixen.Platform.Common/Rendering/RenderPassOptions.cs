using System.Drawing;

namespace Vixen.Platform.Common.Rendering;

public sealed class RenderPassOptions {
    public IPipeline Pipeline { get; set; }
    public string DebugName { get; set; }
    public Color MarkerColor { get; set; }
}
