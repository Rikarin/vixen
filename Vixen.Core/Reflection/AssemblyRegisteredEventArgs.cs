using System.Reflection;

namespace Vixen.Core.Reflection;

/// <summary>
///     An event occurring when an assembly is registered with <see cref="AssemblyRegistry" />.
/// </summary>
public class AssemblyRegisteredEventArgs : EventArgs {
    /// <summary>
    ///     Gets the assembly that has been registered.
    /// </summary>
    /// <value>The assembly.</value>
    public Assembly Assembly { get; }

    /// <summary>
    ///     Gets the new categories registered for the specified <see cref="Assembly" />
    /// </summary>
    /// <value>The categories.</value>
    public HashSet<string> Categories { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="AssemblyRegisteredEventArgs" /> class.
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    /// <param name="categories">The categories.</param>
    public AssemblyRegisteredEventArgs(Assembly assembly, HashSet<string> categories) {
        Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
        Categories = categories ?? throw new ArgumentNullException(nameof(categories));
    }
}