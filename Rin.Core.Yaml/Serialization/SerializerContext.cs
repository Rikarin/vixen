using Rin.Core.Reflection.TypeDescriptors;
using Rin.Core.Yaml.Events;
using Rin.Core.Yaml.Schemas;
using Serilog;

namespace Rin.Core.Yaml.Serialization;

/// <summary>
///     A context used while deserializing.
/// </summary>
public class SerializerContext : ITagTypeResolver {
    /// <summary>
    ///     Gets the dictionary of custom properties associated to this context.
    /// </summary>
    public PropertyContainer Properties;

    internal int AnchorCount;

    /// <summary>
    ///     Gets a value indicating whether we are in the context of serializing.
    /// </summary>
    /// <value><c>true</c> if we are in the context of serializing; otherwise, <c>false</c>.</value>
    public bool IsSerializing => Writer != null;

    /// <summary>
    ///     Gets the logger.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    ///     Gets the settings.
    /// </summary>
    /// <value>The settings.</value>
    public SerializerSettings Settings => Serializer.Settings;

    /// <summary>
    ///     Gets the schema.
    /// </summary>
    /// <value>The schema.</value>
    public IYamlSchema Schema => Settings.Schema;

    /// <summary>
    ///     Gets the serializer.
    /// </summary>
    /// <value>The serializer.</value>
    public Serializer Serializer { get; }

    /// <summary>
    ///     Gets or sets the reader used while deserializing.
    /// </summary>
    /// <value>The reader.</value>
    public EventReader Reader { get; set; }

    /// <summary>
    ///     Gets the object serializer backend.
    /// </summary>
    /// <value>The object serializer backend.</value>
    public IObjectSerializerBackend ObjectSerializerBackend { get; private set; }

    /// <summary>
    ///     Gets or sets a value indicating whether errors are allowed.
    /// </summary>
    /// <value>
    ///     <c>true</c> if errors are allowed; otherwise, <c>false</c>.
    /// </value>
    public bool AllowErrors { get; set; }

    /// <summary>
    ///     Gets a value indicating whether the deserialization has generated some remap.
    /// </summary>
    /// <value><c>true</c> if the deserialization has generated some remap; otherwise, <c>false</c>.</value>
    public bool HasRemapOccurred { get; internal set; }

    /// <summary>
    ///     Gets or sets the member mask that will be used to filter <see cref="DataMemberAttribute.Mask" />.
    /// </summary>
    /// <value>
    ///     The member mask.
    /// </value>
    public uint MemberMask { get; }

    /// <summary>
    ///     Gets or sets the type of the create.
    /// </summary>
    /// <value>The type of the create.</value>
    public IObjectFactory ObjectFactory { get; set; }

    /// <summary>
    ///     Gets or sets the writer used while deserializing.
    /// </summary>
    /// <value>The writer.</value>
    public IEventEmitter? Writer { get; set; }

    /// <summary>
    ///     Gets the emitter.
    /// </summary>
    /// <value>The emitter.</value>
    public IEmitter Emitter { get; internal set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="SerializerContext" /> class.
    /// </summary>
    /// <param name="serializer">The serializer.</param>
    /// <param name="serializerContextSettings">The serializer context settings.</param>
    internal SerializerContext(Serializer serializer, SerializerContextSettings? serializerContextSettings) {
        Serializer = serializer;
        ObjectFactory = serializer.Settings.ObjectFactory;
        ObjectSerializerBackend = serializer.Settings.ObjectSerializerBackend;
        var contextSettings = serializerContextSettings ?? SerializerContextSettings.Default;
        Logger = contextSettings.Logger;
        MemberMask = contextSettings.MemberMask;
        Properties = contextSettings.Properties;
    }

    /// <summary>
    ///     Finds the type descriptor for the specified type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>An instance of <see cref="ITypeDescriptor" />.</returns>
    public ITypeDescriptor FindTypeDescriptor(Type type) => Serializer.TypeDescriptorFactory.Find(type);

    /// <summary>
    ///     Resolves a type from the specified tag.
    /// </summary>
    /// <param name="tagName">Name of the tag.</param>
    /// <param name="isAlias">True if tag is an alias.</param>
    /// <returns>Type.</returns>
    public Type TypeFromTag(string tagName, out bool isAlias) =>
        Serializer.Settings.AssemblyRegistry.TypeFromTag(tagName, out isAlias);

    /// <summary>
    ///     Resolves a tag from a type
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>The associated tag</returns>
    public string TagFromType(Type type) => Serializer.Settings.AssemblyRegistry.TagFromType(type);

    /// <summary>
    ///     Resolves a type from the specified typename using registered assemblies.
    /// </summary>
    /// <param name="typeFullName">Full name of the type.</param>
    /// <returns>The type of null if not found</returns>
    public Type ResolveType(string typeFullName) => Serializer.Settings.AssemblyRegistry.ResolveType(typeFullName);

    /// <summary>
    ///     Resolves a type and assembly from the full name.
    /// </summary>
    /// <param name="typeFullName">Full name of the type.</param>
    public void ParseType(string typeFullName, out string typeName, out string assemblyName) {
        Serializer.Settings.AssemblyRegistry.ParseType(typeFullName, out typeName, out assemblyName);
    }

    /// <summary>
    ///     Gets the default tag and value for the specified <see cref="Scalar" />. The default tag can be different from a
    ///     actual tag of this <see cref="NodeEvent" />.
    /// </summary>
    /// <param name="scalar">The scalar event.</param>
    /// <param name="defaultTag">The default tag decoded from the scalar.</param>
    /// <param name="value">The value extracted from a scalar.</param>
    /// <returns>System.String.</returns>
    public bool TryParseScalar(Scalar scalar, out string defaultTag, out object value) =>
        Settings.Schema.TryParse(scalar, true, out defaultTag, out value);
}
