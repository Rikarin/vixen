namespace Rin.Core.Serialization;

/// <summary>
///     Enumerates the different mode of serialization (either serialization or deserialization).
/// </summary>
public enum ArchiveMode {
    /// <summary>
    ///     The serializer is in serialize mode.
    /// </summary>
    Serialize,

    /// <summary>
    ///     The serializer is in deserialize mode.
    /// </summary>
    Deserialize
}
