using Vixen.Core.Reflection.MemberDescriptors;

namespace Vixen.Core.Reflection;

public class DefaultKeyComparer : IComparer<object> {
    public int Compare(object? x, object? y) {
        if (x is IMemberDescriptor left && y is IMemberDescriptor right) {
            // If order is defined, first order by order
            if (left.Order.HasValue | right.Order.HasValue) {
                var leftOrder = left.Order ?? int.MaxValue;
                var rightOrder = right.Order ?? int.MaxValue;
                return leftOrder.CompareTo(rightOrder);
            }

            // else order by name (dynamic members, etc...)
            return left.DefaultNameComparer.Compare(left.Name, right.Name);
        }

        if (x is string sx && y is string sy) {
            return string.CompareOrdinal(sx, sy);
        }

        if (x is IComparable leftComparable) {
            return leftComparable.CompareTo(y);
        }

        var rightComparable = y as IComparable;
        return rightComparable?.CompareTo(y) ?? 0;
    }
}
