namespace Rin.Core.Annotations;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
[BaseTypeRequired(typeof(Attribute))]
public sealed class BaseTypeRequiredAttribute : Attribute {
    public Type BaseType { get; private set; }

    public BaseTypeRequiredAttribute(Type baseType) {
        BaseType = baseType;
    }
}
