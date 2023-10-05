using Rin.Core.Abstractions;

namespace Rin.Core.General;

public sealed class ApplicationOptions {
    public string Name { get; set; }

    public ThreadingPolicy ThreadingPolicy { get; set; }
}
