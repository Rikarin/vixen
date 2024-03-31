namespace Rin.Core.Reflection;

/// <summary>
///     Base interface for renaming members.
/// </summary>
public interface IMemberNamingConvention {
    /// <summary>
    ///     Gets the comparer used for this member name.
    /// </summary>
    /// <value>The comparer.</value>
    StringComparer Comparer { get; }

    /// <summary>
    ///     Converts the specified member name.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>System.String.</returns>
    string Convert(string name);
}
