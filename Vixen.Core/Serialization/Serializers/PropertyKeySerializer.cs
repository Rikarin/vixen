using System.Reflection;
using Vixen.Core.Reflection;

namespace Vixen.Core.Serialization.Serializers;

public class PropertyKeySerializer<T> : DataSerializer<T> where T : PropertyKey {
    public override void Serialize(ref T? obj, ArchiveMode mode, SerializationStream stream) {
        if (mode == ArchiveMode.Serialize) {
            stream.Write(obj.Name);
            stream.Write(obj.OwnerType.AssemblyQualifiedName);
        } else {
            var parameterName = stream.ReadString();
            var ownerTypeName = stream.ReadString();
            var ownerType = AssemblyRegistry.GetType(ownerTypeName);

            obj = (T)ownerType.GetTypeInfo().GetDeclaredField(parameterName).GetValue(null);
        }
    }
}
