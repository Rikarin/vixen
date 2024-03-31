using Rin.Core.Reflection.MemberDescriptors;
using Rin.Core.Reflection.TypeDescriptors;

namespace Rin.Core.Yaml.Serialization;

/// <summary>
///     The object context used when serializing/deserializing an object instance. See remarks.
/// </summary>
/// <remarks>
///     <para>
///         When serializing, this struct contains the <see cref="Instance" /> of the object to serialize, the type, the
///         tag to use
///         and the style, as well as providing access to the <see cref="SerializerContext" />.
///     </para>
///     <para>
///         When deserializing, this struct will contain the expected type to deserialize and if not null, the instance of
///         an object
///         that will receive deserialization of its members (in case the instance cannot be created).
///     </para>
/// </remarks>
public struct ObjectContext {
    /// <summary>
    ///     The serializer context associated to this object context.
    /// </summary>
    public readonly SerializerContext SerializerContext;

    /// <summary>
    ///     The dictionary containing custom properties for this context.
    /// </summary>
    public PropertyContainer Properties;

    /// <summary>
    ///     Gets the current YAML reader. Equivalent to calling directly
    ///     <see cref="Serialization.SerializerContext.Reader" />.
    /// </summary>
    /// <value>The current YAML reader.</value>
    public EventReader Reader => SerializerContext.Reader;

    /// <summary>
    ///     Gets the writer used while deserializing. Equivalent to calling directly
    ///     <see cref="Serialization.SerializerContext.Writer" />.
    /// </summary>
    /// <value>The writer.</value>
    public IEventEmitter Writer => SerializerContext.Writer;

    /// <summary>
    ///     Gets the settings. Equivalent to calling directly
    ///     <see cref="Serialization.SerializerContext.Settings" />.
    /// </summary>
    /// <value>The settings.</value>
    public SerializerSettings Settings => SerializerContext.Settings;

    /// <summary>
    ///     Gets the object serializer backend.
    /// </summary>
    /// <value>The object serializer backend.</value>
    public IObjectSerializerBackend ObjectSerializerBackend => SerializerContext.ObjectSerializerBackend;

    /// <summary>
    ///     The instance link to this context.
    /// </summary>
    public object Instance { get; set; }

    /// <summary>
    ///     The expected type descriptor.
    /// </summary>
    public ITypeDescriptor? Descriptor { get; set; }

    /// <summary>
    ///     The type descriptor of the parent of the instance type.
    /// </summary>
    public ITypeDescriptor? ParentTypeDescriptor { get; set; }

    /// <summary>
    ///     The type descriptor of the parent's member that generates this type of instance.
    /// </summary>
    public IMemberDescriptor? ParentTypeMemberDescriptor { get; set; }

    /// <summary>
    ///     The tag used when serializing.
    /// </summary>
    public string Tag { get; set; }

    /// <summary>
    ///     The anchor used when serializing.
    /// </summary>
    public string Anchor { get; set; }

    /// <summary>
    ///     The style used when serializing.
    /// </summary>
    public DataStyle Style { get; set; }

    /// <summary>
    ///     The style used when serializing scalars.
    /// </summary>
    public ScalarStyle ScalarStyle { get; set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ObjectContext" /> struct.
    /// </summary>
    /// <param name="serializerContext">The serializer context.</param>
    /// <param name="instance">The instance.</param>
    /// <param name="descriptor">The descriptor.</param>
    public ObjectContext(
        SerializerContext serializerContext,
        object instance,
        ITypeDescriptor? descriptor,
        ITypeDescriptor? parentTypeDescriptor = null,
        IMemberDescriptor? parentTypeMemberDescriptor = null
    ) : this() {
        SerializerContext = serializerContext;
        Instance = instance;
        Descriptor = descriptor;
        ParentTypeDescriptor = parentTypeDescriptor;
        ParentTypeMemberDescriptor = parentTypeMemberDescriptor;
        Properties = new();
    }
}
