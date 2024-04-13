namespace Vixen.Core.Yaml.Serialization;

/// <summary>
///     Creates instances of types.
/// </summary>
/// <remarks>
///     This interface allows to provide a custom logic for creating instances during deserialization.
/// </remarks>
public interface IObjectFactory {
    /// <summary>
    ///     Creates an instance of the specified type. Throws with an appropriate exception if the type cannot be created.
    /// </summary>
    object Create(Type type);
}
