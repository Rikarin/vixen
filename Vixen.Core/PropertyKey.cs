using System.Diagnostics;
using System.Reflection;
using Vixen.Core.Serialization;
using Vixen.Core.Serialization.Serializers;

namespace Vixen.Core;

/// <summary>
///     A class that represents a tag property.
/// </summary>
[DataContract]
[DataSerializer(typeof(PropertyKeySerializer<>), Mode = DataSerializerGenericMode.Type)]
[DebuggerDisplay("{" + nameof(Name) + "}")]
public abstract class PropertyKey : IComparable {
    DefaultValueMetadata defaultValueMetadata;

    /// <summary>
    ///     Gets the name of this key.
    /// </summary>
    public string Name { get; protected set; }

    /// <summary>
    ///     Gets the metadatas.
    /// </summary>
    [DataMemberIgnore]
    public PropertyKeyMetadata[] Metadata { get; }

    /// <summary>
    ///     Gets the type of the owner.
    /// </summary>
    /// <value>
    ///     The type of the owner.
    /// </value>
    [DataMemberIgnore]
    public Type OwnerType { get; protected set; }

    /// <summary>
    ///     Gets the type of the property.
    /// </summary>
    /// <value>
    ///     The type of the property.
    /// </value>
    [DataMemberIgnore]
    public Type PropertyType { get; protected set; }

    public abstract bool IsValueType { get; }

    /// <summary>
    ///     Gets the default value metadata.
    /// </summary>
    // [DataMemberIgnore]
    internal DefaultValueMetadata DefaultValueMetadata {
        get => defaultValueMetadata;
        set {
            defaultValueMetadata = value;
            PropertyUpdateCallback = defaultValueMetadata.PropertyUpdateCallback;
        }
    }

    /// <summary>
    ///     Gets the validate value metadata (may be null).
    /// </summary>
    /// <value>The validate value metadata.</value>
    [DataMemberIgnore]
    internal ValidateValueMetadata ValidateValueMetadata { get; private set; }

    /// <summary>
    ///     Gets the object invalidation metadata (may be null).
    /// </summary>
    /// <value>The object invalidation metadata.</value>
    [DataMemberIgnore]
    internal ObjectInvalidationMetadata ObjectInvalidationMetadata { get; private set; }

    /// <summary>
    ///     Gets the accessor metadata (may be null).
    /// </summary>
    /// <value>The accessor metadata.</value>
    [DataMemberIgnore]
    internal AccessorMetadata AccessorMetadata { get; private set; }

    /// <summary>Gets the property update callback.</summary>
    /// <value>The property update callback.</value>
    [DataMemberIgnore]
    internal PropertyContainer.PropertyUpdatedDelegate? PropertyUpdateCallback { get; private set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PropertyKey" /> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="propertyType">Type of the property.</param>
    /// <param name="ownerType">Type of the owner.</param>
    /// <param name="metadata">The metadatas.</param>
    protected PropertyKey(
        string name,
        Type propertyType,
        Type ownerType,
        params PropertyKeyMetadata[] metadata
    ) {
        if (name == null) {
            throw new ArgumentNullException(nameof(name));
        }

        Name = name;
        PropertyType = propertyType;
        OwnerType = ownerType;
        Metadata = metadata;
        SetupMetadatas();
    }

    public int CompareTo(object obj) {
        var key = obj as PropertyKey;
        if (key == null) {
            return 0;
        }

        return string.Compare(Name, key.Name, StringComparison.OrdinalIgnoreCase);
    }

    public override string ToString() => Name;

    protected virtual void SetupMetadatas() {
        foreach (var metadata in Metadata) {
            SetupMetadata(metadata);
        }
    }

    protected virtual void SetupMetadata(PropertyKeyMetadata metadata) {
        if (metadata is DefaultValueMetadata) {
            DefaultValueMetadata = (DefaultValueMetadata)metadata;
        }

        if (metadata is AccessorMetadata) {
            AccessorMetadata = (AccessorMetadata)metadata;
        }

        if (metadata is ValidateValueMetadata) {
            ValidateValueMetadata = (ValidateValueMetadata)metadata;
        }

        if (metadata is ObjectInvalidationMetadata) {
            ObjectInvalidationMetadata = (ObjectInvalidationMetadata)metadata;
        }
    }

    internal abstract PropertyContainer.ValueHolder CreateValueHolder(object value);
}

/// <summary>
///     A class that represents a typed tag propety.
/// </summary>
/// <typeparam name="T">Type of the property</typeparam>
public sealed class PropertyKey<T> : PropertyKey {
    static readonly bool IsValueTypeGeneric = typeof(T).GetTypeInfo().IsValueType;

    /// <inheritdoc />
    public override bool IsValueType => IsValueTypeGeneric;

    /// <summary>
    ///     Gets the default value metadata.
    /// </summary>
    public DefaultValueMetadata<T> DefaultValueMetadataT => (DefaultValueMetadata<T>)DefaultValueMetadata;

    /// <summary>
    ///     Gets the validate value metadata (may be null).
    /// </summary>
    /// <value>The validate value metadata.</value>
    public ValidateValueMetadata<T> ValidateValueMetadataT => (ValidateValueMetadata<T>)ValidateValueMetadata;

    /// <summary>
    ///     Gets the object invalidation metadata (may be null).
    /// </summary>
    /// <value>The object invalidation metadata.</value>
    public ObjectInvalidationMetadata<T> ObjectInvalidationMetadataT =>
        (ObjectInvalidationMetadata<T>)ObjectInvalidationMetadata;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PropertyKey{T}" /> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="ownerType">Type of the owner.</param>
    /// <param name="metadata">The metadatas.</param>
    public PropertyKey(string name, Type ownerType, params PropertyKeyMetadata[] metadata)
        : base(name, typeof(T), ownerType, GenerateDefaultData(metadata)) { }

    static PropertyKeyMetadata[] GenerateDefaultData(PropertyKeyMetadata[] metadata) {
        if (metadata == null) {
            return new[] { new StaticDefaultValueMetadata<T>(default(T)) };
        }

        var defaultMetaData = metadata.OfType<DefaultValueMetadata>().FirstOrDefault();
        if (defaultMetaData == null) {
            var newMetaData = new PropertyKeyMetadata[metadata.Length + 1];
            metadata.CopyTo(newMetaData, 1);
            newMetaData[0] = new StaticDefaultValueMetadata<T>(default(T));
            return newMetaData;
        }

        return metadata;
    }

    internal override PropertyContainer.ValueHolder CreateValueHolder(object value) =>
        new PropertyContainer.ValueHolder<T>((T)value);
}
