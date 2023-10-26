namespace Rin.Core.Serialization;

/// <summary>
///     Describes how to serialize and deserialize an object without knowing its type.
///     Used as a common base class for all data serializers.
/// </summary>
partial class DataSerializer {
    // Binary format version, needs to be bumped in case of big changes in serialization formats (i.e. primitive types).
    public const int BinaryFormatVersion = 4 * 1000000 // Major version: any number is ok
        + 0 * 10000 // Minor version: supported range: 0-99
        + 0 * 100 // Patch version: supported range: 0-99
        + 1; // Bump ID: supported range: 0-99
}
