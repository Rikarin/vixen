using Vixen.Core.Reflection.TypeDescriptors;

namespace Vixen.Core.Reflection;

/// <summary>
///     A default implementation for the <see cref="ITypeDescriptorFactory" />.
/// </summary>
public class TypeDescriptorFactory : ITypeDescriptorFactory {
    readonly IComparer<object> keyComparer;
    readonly Dictionary<Type, ITypeDescriptor> registeredDescriptors = new();
    readonly bool emitDefaultValues;
    readonly IMemberNamingConvention namingConvention;

    /// <summary>
    ///     The default type descriptor factory.
    /// </summary>
    public static readonly TypeDescriptorFactory Default = new();

    public IAttributeRegistry AttributeRegistry { get; }

    public TypeDescriptorFactory() : this(new AttributeRegistry()) { }

    public TypeDescriptorFactory(IAttributeRegistry attributeRegistry)
        : this(attributeRegistry, false, new DefaultNamingConvention()) { }

    public TypeDescriptorFactory(
        IAttributeRegistry attributeRegistry,
        bool emitDefaultValues,
        IMemberNamingConvention namingConvention
    ) : this(attributeRegistry, emitDefaultValues, namingConvention, new DefaultMemberComparer()) { }

    public TypeDescriptorFactory(
        IAttributeRegistry attributeRegistry,
        bool emitDefaultValues,
        IMemberNamingConvention namingConvention,
        IComparer<object> keyComparer
    ) {
        if (attributeRegistry == null) {
            throw new ArgumentNullException(nameof(attributeRegistry));
        }

        this.keyComparer = keyComparer;
        AttributeRegistry = attributeRegistry;
        this.emitDefaultValues = emitDefaultValues;
        this.namingConvention = namingConvention;
    }

    public ITypeDescriptor? Find(Type? type) {
        if (type == null) {
            return null;
        }

        // Caching is integrated in this class, avoiding a ChainedTypeDescriptorFactory
        ITypeDescriptor? descriptor;
        lock (registeredDescriptors) {
            if (!registeredDescriptors.TryGetValue(type, out descriptor)) {
                descriptor = Create(type);

                // Register this descriptor (before initializing!)
                registeredDescriptors.Add(type, descriptor);

                // Make sure the descriptor is initialized
                descriptor.Initialize(keyComparer);
            }
        }

        return descriptor;
    }

    /// <summary>
    ///     Creates a type descriptor for the specified type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>An instance of type descriptor.</returns>
    protected virtual ITypeDescriptor Create(Type type) {
        ITypeDescriptor descriptor;
        // The order of the descriptors here is important

        if (PrimitiveDescriptor.IsPrimitive(type)) {
            descriptor = new PrimitiveDescriptor(this, type, emitDefaultValues, namingConvention);
        } else if
            (DictionaryDescriptor
             .IsDictionary(type)) // resolve dictionary before collections, as they are also collections
        {
            // IDictionary
            descriptor = new DictionaryDescriptor(this, type, emitDefaultValues, namingConvention);
        } else if (ListDescriptor.IsList(type)) {
            // IList
            descriptor = new ListDescriptor(this, type, emitDefaultValues, namingConvention);
        } else if (SetDescriptor.IsSet(type)) {
            // ISet
            descriptor = new SetDescriptor(this, type, emitDefaultValues, namingConvention);
        }
        // TODO(Jiu): Verify if this can be removed
        // else if (CollectionDescriptor.IsCollection(type)) {
        //     // ICollection
        //     descriptor = new OldCollectionDescriptor(this, type, emitDefaultValues, namingConvention);
        // }
        else if (type.IsArray) {
            if (type.GetArrayRank() == 1 && !type.GetElementType().IsArray) {
                // array[] - only single dimension array is supported
                descriptor = new ArrayDescriptor(this, type, emitDefaultValues, namingConvention);
            } else {
                // multi-dimension array to be treated as a 'standard' object
                descriptor = new NotSupportedObjectDescriptor(this, type, emitDefaultValues, namingConvention);
            }
        } else if (NullableDescriptor.IsNullable(type)) {
            descriptor = new NullableDescriptor(this, type, emitDefaultValues, namingConvention);
        } else {
            // standard object (class or value type)
            descriptor = new ObjectDescriptor(this, type, emitDefaultValues, namingConvention);
        }

        return descriptor;
    }
}
