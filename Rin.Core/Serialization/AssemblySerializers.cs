using System.Reflection;

namespace Rin.Core.Serialization;

public class AssemblySerializers {
    public Assembly Assembly { get; }
    public List<Module> Modules { get; }
    public List<DataContractAlias> DataContractAliases { get; }
    public Dictionary<string, AssemblySerializersPerProfile> Profiles { get; }

    public AssemblySerializers(Assembly assembly) {
        Assembly = assembly;
        Modules = new();
        Profiles = new();
        DataContractAliases = new();
    }

    public override string ToString() => Assembly.ToString();

    public struct DataContractAlias {
        public string Name;
        public Type Type;

        /// <summary>
        ///     True if generated from a <see cref="DataAliasAttribute" />, false if generated from a
        ///     <see cref="DataContractAttribute" />.
        /// </summary>
        public bool IsAlias;

        public DataContractAlias(string name, Type type, bool isAlias) {
            Name = name;
            Type = type;
            IsAlias = isAlias;
        }
    }
}
