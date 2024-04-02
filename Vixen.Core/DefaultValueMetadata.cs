namespace Vixen.Core;

/// <summary>
///     Abstract class that could be overloaded in order to define how to get default value of an
///     <see cref="PropertyKey" />.
/// </summary>
public abstract class DefaultValueMetadata : PropertyKeyMetadata {
    /// <summary>
    ///     Gets a value indicating whether this value is kept.
    /// </summary>
    /// <value>
    ///     <c>true</c> if this value is kept; otherwise, <c>false</c>.
    /// </value>
    public virtual bool KeepValue => false;

    /// <summary>Gets or sets the property update callback.</summary>
    /// <value>The property update callback.</value>
    public PropertyContainer.PropertyUpdatedDelegate PropertyUpdateCallback { get; set; }

    /// <summary>
    ///     Gets the default value of an external property, and specify if this default value should be kept.
    ///     It could be usefull with properties with default values depending of its container, especially if they are long to
    ///     generate.
    ///     An example would be collision data, which should be generated only once.
    /// </summary>
    /// <param name="obj">The property container.</param>
    /// <returns>The default value.</returns>
    public abstract object GetDefaultValue(ref PropertyContainer obj);

    public static StaticDefaultValueMetadata<T> Static<T>(T defaultValue, bool keepDefaultValue = false) =>
        new(defaultValue, keepDefaultValue);

    public static DelegateDefaultValueMetadata<T> Delegate<T>(
        DelegateDefaultValueMetadata<T>.DefaultValueCallback callback
    ) => new(callback);
}
