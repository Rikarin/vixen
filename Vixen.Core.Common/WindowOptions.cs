using System.Drawing;

namespace Vixen.Core.Common;

public sealed class WindowOptions {
    public string Title { get; set; }
    public Size Size { get; set; }
    public bool Decorated { get; set; }
    public bool Fullscreen { get; set; }
    public bool VSync { get; set; }
    public string? IconPath { get; set; }
}
