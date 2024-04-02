namespace Vixen.Core.Extensions;

public static class ArrayExtensions {
    /// <summary>
    ///     Deeply compares of two <see cref="IList{T}" />.
    /// </summary>
    /// <typeparam name="T">Type of the object to compare</typeparam>
    /// <param name="a1">The list1 to compare</param>
    /// <param name="a2">The list2 to compare</param>
    /// <param name="comparer">The comparer to use (or default to the default EqualityComparer for T)</param>
    /// <returns><c>true</c> if the list are equal</returns>
    public static bool ArraysEqual<T>(IList<T>? a1, IList<T>? a2, IEqualityComparer<T>? comparer = null) {
        // This is not really an extension method, maybe it should go somewhere else.
        if (ReferenceEquals(a1, a2)) {
            return true;
        }

        if (a1 == null || a2 == null) {
            return false;
        }

        if (a1.Count != a2.Count) {
            return false;
        }

        comparer ??= EqualityComparer<T>.Default;
        return !a1.Where((t, i) => !comparer.Equals(t, a2[i])).Any();
    }
}
