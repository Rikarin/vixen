namespace Rin.Core.Yaml.Serialization.Serializers;

/// <summary>
///     This serializer is responsible to route to a specific serializer.
/// </summary>
public class RoutingSerializer : ChainedSerializer, IYamlSerializable {
    readonly ISerializerFactorySelector serializerSelector;

    public RoutingSerializer(ISerializerFactorySelector serializerSelector) {
        if (serializerSelector == null) {
            throw new ArgumentNullException(nameof(serializerSelector));
        }

        this.serializerSelector = serializerSelector;
    }

    public sealed override object ReadYaml(ref ObjectContext objectContext) {
        // If value is not null, use its TypeDescriptor otherwise use expected type descriptor
        var instance = objectContext.Instance;
        var typeDescriptorOfValue = instance != null
            ? objectContext.SerializerContext.FindTypeDescriptor(instance.GetType())
            : objectContext.Descriptor;

        var serializer = serializerSelector.GetSerializer(objectContext.SerializerContext, typeDescriptorOfValue);
        return serializer.ReadYaml(ref objectContext);
    }

    public sealed override void WriteYaml(ref ObjectContext objectContext) {
        var serializer = serializerSelector.GetSerializer(objectContext.SerializerContext, objectContext.Descriptor);
        serializer.WriteYaml(ref objectContext);
    }
}
