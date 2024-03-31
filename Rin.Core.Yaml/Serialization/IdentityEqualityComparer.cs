using System.Runtime.CompilerServices;

namespace Rin.Core.Yaml.Serialization;

class IdentityEqualityComparer<T> : IEqualityComparer<T> where T : class {
    public bool Equals(T? left, T? right) => ReferenceEquals(left, right);
    public int GetHashCode(T value) => RuntimeHelpers.GetHashCode(value);
}
