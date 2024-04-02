using Vixen.Core.Serialization;

namespace Vixen.Core.Design;

[DataContract("PackageVersion")]
[DataSerializer(typeof(PackageVersionDataSerializer))]
public sealed partial class PackageVersion {
    internal class PackageVersionDataSerializer : DataSerializer<PackageVersion> {
        /// <inheritdoc />
        public override bool IsBlittable => true;

        /// <inheritdoc />
        public override void Serialize(ref PackageVersion obj, ArchiveMode mode, SerializationStream stream) {
            if (mode == ArchiveMode.Deserialize) {
                string? version = null;
                stream.Serialize(ref version);
                obj = Parse(version);
            } else {
                var version = obj.ToString();
                stream.Serialize(ref version);
            }
        }
    }
}
