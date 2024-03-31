using Rin.Core.Reflection.MemberDescriptors;
using Rin.Core.Reflection.TypeDescriptors;
using System.Reflection;

namespace Rin.Core.Yaml.Serialization;

/// <summary>
///     A dynamic member to allow adding dynamic members to objects (that could store additional properties outside of the
///     instance).
/// </summary>
public abstract class DynamicMemberDescriptorBase : IMemberDescriptor {
    public string Name { get; set; }
    public string OriginalName { get; set; }
    public StringComparer DefaultNameComparer { get; set; }
    public Type Type { get; }
    public Type DeclaringType { get; }

    // TODO: store the proper type descriptor here
    public ITypeDescriptor TypeDescriptor => null;

    public int? Order { get; set; }
    public DataMemberMode Mode { get; set; }
    public abstract bool HasSet { get; }
    public bool IsPublic => true;
    public uint Mask { get; set; }
    public DataStyle Style { get; set; }
    public ScalarStyle ScalarStyle { get; set; }
    public ShouldSerializePredicate ShouldSerialize { get; set; }
    public bool HasDefaultValue => false;
    public object DefaultValue => throw new InvalidOperationException();
    public List<string> AlternativeNames { get; set; }
    public object Tag { get; set; }
    public MemberInfo MemberInfo => null;

    protected DynamicMemberDescriptorBase(string name, Type type, Type declaringType) {
        if (name == null) {
            throw new ArgumentNullException(nameof(name));
        }

        if (type == null) {
            throw new ArgumentNullException(nameof(type));
        }

        Name = name;
        Type = type;
        DeclaringType = declaringType;
        OriginalName = Name;
        Mask = 1;
        ShouldSerialize = ObjectDescriptor.ShouldSerializeDefault;
        DefaultNameComparer = StringComparer.OrdinalIgnoreCase;
    }

    public abstract object Get(object thisObject);

    public abstract void Set(object thisObject, object value);

    public IEnumerable<T> GetCustomAttributes<T>(bool inherit) where T : Attribute {
        yield break;
    }
}
