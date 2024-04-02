namespace Vixen.Core.Yaml.Serialization;

/// <summary>
///     Allows an object to customize how it is serialized and deserialized.
/// </summary>
public interface IYamlSerializable {
    /// <summary>
    ///     Reads this object's state from a YAML parser.
    /// </summary>
    /// <param name="objectContext"></param>
    /// <returns>A instance of the object deserialized from Yaml.</returns>
    object ReadYaml(ref ObjectContext objectContext);

    /// <summary>
    ///     Writes the specified object context to a YAML emitter.
    /// </summary>
    /// <param name="objectContext">The object context.</param>
    void WriteYaml(ref ObjectContext objectContext);
}
