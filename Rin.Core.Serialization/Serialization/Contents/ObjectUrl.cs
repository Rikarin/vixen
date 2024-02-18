namespace Rin.Core.Serialization.Serialization.Contents;

[DataContract]
[Serializable]
[DataSerializer(typeof(Serializer))]
public readonly record struct ObjectUrl(UrlType Type, string Path) {
    public static readonly ObjectUrl Empty = new(UrlType.None, string.Empty);
    public override string ToString() => Path;

    internal class Serializer : DataSerializer<ObjectUrl> {
        public override void Serialize(ref ObjectUrl obj, ArchiveMode mode, SerializationStream stream) {
            if (mode == ArchiveMode.Serialize) {
                stream.Write(obj.Type);
                stream.Write(obj.Path);
            } else {
                var type = stream.Read<UrlType>();
                var path = stream.ReadString();
                obj = new(type, path);
            }
        }
    }
}
