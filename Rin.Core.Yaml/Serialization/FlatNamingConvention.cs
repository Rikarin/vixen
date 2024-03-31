using Rin.Core.Reflection;
using System.Text.RegularExpressions;

namespace Rin.Core.Yaml.Serialization;

/// <summary>
///     A naming convention where all members are transformed from`CamelCase` to `camel_case`.
/// </summary>
public class FlatNamingConvention : IMemberNamingConvention {
    // Code taken from dotliquid/RubyNamingConvention.cs
    readonly Regex regex1 = new(@"([A-Z]+)([A-Z][a-z])");
    readonly Regex regex2 = new(@"([a-z\d])([A-Z])");

    public StringComparer Comparer => StringComparer.OrdinalIgnoreCase;

    public string Convert(string name) => regex2.Replace(regex1.Replace(name, "$1_$2"), "$1_$2").ToLowerInvariant();
}
