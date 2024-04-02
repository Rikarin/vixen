using Vixen.Core.Yaml.Events;

namespace Vixen.Core.Yaml.Serialization.Serializers;

public abstract class ScalarSerializerBase : IYamlSerializable {
    public object ReadYaml(ref ObjectContext objectContext) {
        var scalar = objectContext.Reader.Expect<Scalar>();
        return ConvertFrom(ref objectContext, scalar);
    }

    public abstract object ConvertFrom(ref ObjectContext context, Scalar fromScalar);

    public void WriteYaml(ref ObjectContext objectContext) {
        var value = objectContext.Instance;
        var typeOfValue = value.GetType();

        var context = objectContext.SerializerContext;

        var isSchemaImplicitTag = context.Schema.IsTagImplicit(objectContext.Tag);
        var scalar = new ScalarEventInfo(value, typeOfValue) {
            IsPlainImplicit = isSchemaImplicitTag,
            Style = objectContext.ScalarStyle,
            Anchor = objectContext.Anchor,
            Tag = objectContext.Tag
        };


        if (scalar.Style == ScalarStyle.Any) {
            // Parse default types 
            switch (Type.GetTypeCode(typeOfValue)) {
                case TypeCode.Object:
                case TypeCode.String:
                case TypeCode.Char:
                    break;
                default:
                    scalar.Style = ScalarStyle.Plain;
                    break;
            }
        }

        scalar.RenderedValue = ConvertTo(ref objectContext);

        // Emit the scalar
        WriteScalar(ref objectContext, scalar);
    }

    public abstract string ConvertTo(ref ObjectContext objectContext);

    /// <summary>
    ///     Writes the scalar to the <see cref="SerializerContext.Writer" />. See remarks.
    /// </summary>
    /// <param name="objectContext">The object context.</param>
    /// <param name="scalar">The scalar.</param>
    /// <remarks>
    ///     This method can be overloaded to replace the converted scalar just before writing it.
    /// </remarks>
    protected virtual void WriteScalar(ref ObjectContext objectContext, ScalarEventInfo scalar) {
        // Emit the scalar
        objectContext.SerializerContext.Writer.Emit(scalar);
    }
}
