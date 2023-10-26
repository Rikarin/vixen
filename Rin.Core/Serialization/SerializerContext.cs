namespace Rin.Core.Serialization;

public class SerializerContext {
    public PropertyContainer Tags;

    public SerializerContext() {
        SerializerSelector = SerializerSelector.Default;
        Tags = new PropertyContainer(this);
    }

    /// <summary>
    ///     Gets or sets the serializer.
    /// </summary>
    /// <value>
    ///     The serializer.
    /// </value>
    public SerializerSelector SerializerSelector { get; set; }

    public T Get<T>(PropertyKey<T> key) => Tags.Get(key);

    public void Set<T>(PropertyKey<T> key, T value) {
        Tags.SetObject(key, value);
    }
}
