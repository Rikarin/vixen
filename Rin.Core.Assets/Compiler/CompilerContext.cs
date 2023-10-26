using System.Data;

namespace Rin.Core.Assets.Compiler;

/// <summary>
///     The context used when compiling an asset in a Package.
/// </summary>
public class CompilerContext : IDisposable {
    /// <summary>
    ///     Properties passed on the command line.
    /// </summary>
    public Dictionary<string, string> OptionProperties { get; } = new();

    /// <summary>
    ///     Gets the attributes attached to this context.
    /// </summary>
    /// <value>The attributes.</value>
    public PropertyCollection Properties { get; } = new();

    public CompilerContext Clone() {
        var context = (CompilerContext)MemberwiseClone();
        return context;
    }

    public void Dispose() { }
}
