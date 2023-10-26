using Rin.Core.Reflection;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Rin.Core.Serialization;

public static class DataSerializerFactory {
    internal static object Lock = new();
    internal static int Version;

    // List of serializers per profile
    internal static readonly Dictionary<string, Dictionary<Type, AssemblySerializerEntry>> DataSerializersPerProfile =
        new();

    // List of all the factories
    static readonly List<WeakReference<SerializerSelector>> SerializerSelectors = new();

    // List of registered assemblies
    static readonly List<AssemblySerializers> AssemblySerializers = new();

    static readonly Dictionary<Assembly, AssemblySerializers> AvailableAssemblySerializers = new();

    static readonly Dictionary<string, Type> DataContractAliasMapping = new();

    public static void RegisterSerializerSelector(SerializerSelector serializerSelector) {
        SerializerSelectors.Add(new(serializerSelector));
    }

    public static AssemblySerializerEntry GetSerializer(string profile, Type type) {
        lock (Lock) {
            if (
                !DataSerializersPerProfile.TryGetValue(profile, out var serializers)
                || !serializers.TryGetValue(type, out var assemblySerializerEntry)
            ) {
                return default;
            }

            return assemblySerializerEntry;
        }
    }

    public static void RegisterSerializationAssembly([NotNull] AssemblySerializers assemblySerializers) {
        lock (Lock) {
            // Register it (so that we can get it back if unregistered)
            if (!AvailableAssemblySerializers.ContainsKey(assemblySerializers.Assembly)) {
                AvailableAssemblySerializers.Add(assemblySerializers.Assembly, assemblySerializers);
            }

            // Check if already loaded
            if (AssemblySerializers.Contains(assemblySerializers)) {
                return;
            }

            // Update existing SerializerSelector
            AssemblySerializers.Add(assemblySerializers);
        }

        // Run module ctor
        foreach (var module in assemblySerializers.Modules) {
            ModuleRuntimeHelpers.RunModuleConstructor(module);
        }

        lock (Lock) {
            RegisterSerializers(assemblySerializers);
            Version++;

            // Invalidate each serializer selector (to force them to rebuild combined list of serializers)
            foreach (var weakSerializerSelector in SerializerSelectors) {
                if (weakSerializerSelector.TryGetTarget(out var serializerSelector)) {
                    serializerSelector.Invalidate();
                }
            }
        }
    }

    public static void RegisterSerializationAssembly(Assembly assembly) {
        lock (Lock) {
            if (AvailableAssemblySerializers.TryGetValue(assembly, out var assemblySerializers)) {
                RegisterSerializationAssembly(assemblySerializers);
            }
        }
    }

    public static void UnregisterSerializationAssembly(Assembly assembly) {
        lock (Lock) {
            var removedAssemblySerializer = AssemblySerializers.FirstOrDefault(x => x.Assembly == assembly);
            if (removedAssemblySerializer == null) {
                return;
            }

            AssemblySerializers.Remove(removedAssemblySerializer);

            // Unregister data contract aliases
            foreach (var dataContractAliasEntry in removedAssemblySerializer.DataContractAliases) {
                // TODO: Warning, exception or override if collision? (currently exception, easiest since we can remove them without worry when unloading assembly)
                DataContractAliasMapping.Remove(dataContractAliasEntry.Name);
            }

            // Rebuild serializer list
            // TODO: For now, we simply reregister all assemblies one-by-one, but it can easily be improved if it proves to be unefficient (for now it shouldn't happen often so probably not a big deal)
            DataSerializersPerProfile.Clear();
            DataContractAliasMapping.Clear();

            foreach (var assemblySerializer in AssemblySerializers) {
                RegisterSerializers(assemblySerializer);
            }

            Version++;

            foreach (var weakSerializerSelector in SerializerSelectors) {
                if (weakSerializerSelector.TryGetTarget(out var serializerSelector)) {
                    serializerSelector.Invalidate();
                }
            }
        }
    }

    public static AssemblySerializers GetAssemblySerializers(Assembly assembly) {
        lock (Lock) {
            AvailableAssemblySerializers.TryGetValue(assembly, out var assemblySerializers);
            return assemblySerializers;
        }
    }

    static void RegisterSerializers(AssemblySerializers assemblySerializers) {
        // Register data contract aliases
        foreach (var dataContractAliasEntry in assemblySerializers.DataContractAliases) {
            try {
                // TODO: Warning, exception or override if collision? (currently exception)
                DataContractAliasMapping.Add(dataContractAliasEntry.Name, dataContractAliasEntry.Type);
            } catch (Exception) {
                throw new InvalidOperationException(
                    $"Two different classes have the same DataContract Alias [{dataContractAliasEntry.Name}]: {dataContractAliasEntry.Type} and {DataContractAliasMapping[dataContractAliasEntry.Name]}"
                );
            }
        }

        // Register serializers
        foreach (var assemblySerializerPerProfile in assemblySerializers.Profiles) {
            var profile = assemblySerializerPerProfile.Key;

            if (!DataSerializersPerProfile.TryGetValue(profile, out var dataSerializers)) {
                dataSerializers = new();
                DataSerializersPerProfile.Add(profile, dataSerializers);
            }

            foreach (var assemblySerializer in assemblySerializerPerProfile.Value) {
                if (!dataSerializers.ContainsKey(assemblySerializer.ObjectType)) {
                    dataSerializers.Add(assemblySerializer.ObjectType, assemblySerializer);
                }
            }
        }
    }

    internal static Type? GetTypeFromAlias(string alias) {
        lock (Lock) {
            DataContractAliasMapping.TryGetValue(alias, out var type);
            return type;
        }
    }
}
