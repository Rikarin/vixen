namespace Vixen.Core.Reflection;

/// <summary>
///     A naming convention where all members are outputed as-is.
/// </summary>
public class DefaultNamingConvention : IMemberNamingConvention {
    public StringComparer Comparer => StringComparer.Ordinal;

    public string Convert(string name) => name;
}
