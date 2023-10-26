using System.Runtime.CompilerServices;

namespace Rin.Core;

/// <summary>
///     A Comparator to use <see cref="object.ReferenceEquals" /> method.
/// </summary>
/// <typeparam name="T">Type of the comparer</typeparam>
public class ReferenceEqualityComparer<T> : EqualityComparer<T> where T : class {
    static IEqualityComparer<T>? defaultComparer;

    /// <summary>
    ///     Gets the default.
    /// </summary>
    public new static IEqualityComparer<T> Default => defaultComparer ??= new ReferenceEqualityComparer<T>();

    /// <inheritdoc />
    public override bool Equals(T x, T y) => ReferenceEquals(x, y);

    /// <inheritdoc />
    public override int GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
}
