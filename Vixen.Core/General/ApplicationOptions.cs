using System.Drawing;
using Vixen.Core.Common;

namespace Vixen.Core.General;

public sealed class ApplicationOptions {
    public string Name { get; set; }
    public Size WindowSize { get; set; }
    public bool VSync { get; set; }
    
    // TODO: finish this

    public ThreadingPolicy ThreadingPolicy { get; set; }
}
