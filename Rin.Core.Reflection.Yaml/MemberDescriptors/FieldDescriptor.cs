using Rin.Core.Reflection.TypeDescriptors;
using System.Reflection;

namespace Rin.Core.Reflection.MemberDescriptors;

/// <summary>
///     A <see cref="IMemberDescriptor" /> for a <see cref="FieldInfo" />
/// </summary>
public class FieldDescriptor : MemberDescriptorBase {
    /// <summary>
    ///     Gets the property information attached to this instance.
    /// </summary>
    /// <value>The property information.</value>
    public FieldInfo FieldInfo { get; }

    public override Type Type => FieldInfo.FieldType;

    public override bool IsPublic => FieldInfo.IsPublic;

    public override bool HasSet => true;

    public FieldDescriptor(ITypeDescriptor typeDescriptor, FieldInfo fieldInfo, StringComparer defaultNameComparer)
        : base(fieldInfo, defaultNameComparer) {
        if (fieldInfo == null) {
            throw new ArgumentNullException(nameof(fieldInfo));
        }

        FieldInfo = fieldInfo;
        TypeDescriptor = typeDescriptor;
    }

    public override object Get(object thisObject) => FieldInfo.GetValue(thisObject);

    public override void Set(object thisObject, object value) {
        FieldInfo.SetValue(thisObject, value);
    }

    public override IEnumerable<T> GetCustomAttributes<T>(bool inherit) => FieldInfo.GetCustomAttributes<T>(inherit);

    /// <summary>
    ///     Returns a <see cref="System.String" /> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
    public override string ToString() =>
        $"Field [{Name}] from Type [{(FieldInfo.DeclaringType != null ? FieldInfo.DeclaringType.FullName : string.Empty)}]";
}
