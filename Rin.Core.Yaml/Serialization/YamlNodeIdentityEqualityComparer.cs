namespace Rin.Core.Yaml.Serialization;

/// <summary>
///     Comparer that is based on identity comparisons.
/// </summary>
public sealed class YamlNodeIdentityEqualityComparer : IEqualityComparer<YamlNode> {
    #region IEqualityComparer<YamlNode> Members

    /// <summary />
    public bool Equals(YamlNode x, YamlNode y) => ReferenceEquals(x, y);

    /// <summary />
    public int GetHashCode(YamlNode obj) => obj.GetHashCode();

    #endregion
}
