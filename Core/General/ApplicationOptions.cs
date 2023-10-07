using Rin.Core.Abstractions;
using System.Drawing;

namespace Rin.Core.General;

public sealed class ApplicationOptions {
    public string Name { get; set; }
    public Size WindowSize { get; set; }
    
    // TODO: finish this

    public ThreadingPolicy ThreadingPolicy { get; set; }
}
