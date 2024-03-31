using Rin.Core.Reflection.MemberDescriptors;
using System.Reflection;

namespace Rin.Core.Reflection.TypeDescriptors;

/// <summary>
///     Describes a descriptor for a primitive (bool, char, sbyte, byte, int, uint, long, ulong, float, double, decimal,
///     string, DateTime).
/// </summary>
public class PrimitiveDescriptor : ObjectDescriptor {
    static readonly List<IMemberDescriptor> EmptyMembers = new();
    readonly Dictionary<string, object> enumRemap;

    public override DescriptorCategory Category => DescriptorCategory.Primitive;

    public PrimitiveDescriptor(
        ITypeDescriptorFactory factory,
        Type type,
        bool emitDefaultValues,
        IMemberNamingConvention namingConvention
    )
        : base(factory, type, emitDefaultValues, namingConvention) {
        if (!IsPrimitive(type)) {
            throw new ArgumentException("Type [{0}] is not a primitive");
        }

        // Handle remap for enum items
        if (type.IsEnum) {
            foreach (var member in type.GetFields(BindingFlags.Public | BindingFlags.Static)) {
                var attributes = AttributeRegistry.GetAttributes(member);
                foreach (var attribute in attributes) {
                    if (attribute is DataAliasAttribute aliasAttribute) {
                        if (enumRemap == null) {
                            enumRemap = new(StringComparer.OrdinalIgnoreCase);
                        }

                        enumRemap[aliasAttribute.Name] = member.GetValue(null);
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Parses the enum and trying to use remap if any declared.
    /// </summary>
    /// <param name="enumAsText">The enum as text.</param>
    /// <param name="remapped">if set to <c>true</c> the enum was remapped.</param>
    /// <returns>System.Object.</returns>
    public object ParseEnum(string enumAsText, out bool remapped) {
        remapped = false;
        if (enumRemap != null && enumRemap.TryGetValue(enumAsText, out var value)) {
            remapped = true;
            return value;
        }

        return Enum.Parse(Type, enumAsText, true);
    }

    /// <summary>
    ///     Determines whether the specified type is a primitive.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns><c>true</c> if the specified type is primitive; otherwise, <c>false</c>.</returns>
    public static bool IsPrimitive(Type type) {
        switch (Type.GetTypeCode(type)) {
            case TypeCode.Object:
            case TypeCode.Empty:
                return type == typeof(object)
                    || type == typeof(string)
                    || type == typeof(TimeSpan)
                    || type == typeof(DateTime);
        }

        return true;
    }

    protected override List<IMemberDescriptor> PrepareMembers() => EmptyMembers;
}
