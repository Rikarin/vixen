using Serilog;
using System.Reflection;

namespace Rin.Core.Reflection;

public static class AssemblyRegistry {
    // static readonly ILogger log = Log.ForContext<();
    static readonly object Lock = new();
    static readonly Dictionary<string, HashSet<Assembly>> MapCategoryToAssemblies = new();

    static readonly Dictionary<Assembly, HashSet<string>> MapAssemblyToCategories = new();

    static readonly Dictionary<Assembly, ScanTypes> AssemblyToScanTypes = new();
    static readonly Dictionary<string, Assembly> AssemblyNameToAssembly = new(StringComparer.OrdinalIgnoreCase);

    static AssemblyRegistry() {
        Register(typeof(AssemblyRegistry).Assembly, "Core");
    }

    /// <summary>
    ///     Gets a type by its typename already loaded in the assembly registry.
    /// </summary>
    /// <param name="fullyQualifiedTypeName">The typename</param>
    /// <param name="throwOnError"></param>
    /// <returns>The type instance or null if not found.</returns>
    /// <seealso cref="Type.GetType(string,bool)" />
    /// <seealso cref="Assembly.GetType(string,bool)" />
    public static Type? GetType(string fullyQualifiedTypeName, bool throwOnError = true) {
        var assemblyIndex = fullyQualifiedTypeName.IndexOf(',');
        if (assemblyIndex < 0) {
            throw new ArgumentException(
                $"Invalid full type name [{fullyQualifiedTypeName}], expecting an assembly name",
                nameof(fullyQualifiedTypeName)
            );
        }

        var typeName = fullyQualifiedTypeName[..assemblyIndex];
        var assemblyName = new AssemblyName(fullyQualifiedTypeName[(assemblyIndex + 1)..]);
        lock (Lock) {
            if (AssemblyNameToAssembly.TryGetValue(assemblyName.Name!, out var assembly)) {
                return assembly.GetType(typeName, throwOnError, false);
            }
        }

        // Fallback to default lookup
        return Type.GetType(fullyQualifiedTypeName, throwOnError, false);
    }

    /// <summary>
    ///     Finds all registered assemblies.
    /// </summary>
    /// <returns>A set of all assembly registered.</returns>
    /// <exception cref="System.ArgumentNullException">categories</exception>
    public static HashSet<Assembly> FindAll() {
        lock (Lock) {
            return new(MapAssemblyToCategories.Keys);
        }
    }

    /// <summary>
    ///     Finds registered assemblies that are associated with the specified categories.
    /// </summary>
    /// <param name="categories">The categories.</param>
    /// <returns>A set of assembly associated with the specified categories.</returns>
    /// <exception cref="System.ArgumentNullException">categories</exception>
    public static HashSet<Assembly> Find(IEnumerable<string> categories) {
        var assemblies = new HashSet<Assembly>();
        lock (Lock) {
            foreach (var category in categories) {
                if (MapCategoryToAssemblies.TryGetValue(category, out var assembliesFound)) {
                    foreach (var assembly in assembliesFound) {
                        assemblies.Add(assembly);
                    }
                }
            }
        }

        return assemblies;
    }

    /// <summary>
    ///     Finds registered categories that are associated with the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    /// <returns>A set of category associated with the specified assembly.</returns>
    /// <exception cref="System.ArgumentNullException">categories</exception>
    public static HashSet<string> FindCategories(Assembly assembly) {
        var categories = new HashSet<string>();
        lock (Lock) {
            if (MapAssemblyToCategories.TryGetValue(assembly, out var categoriesFound)) {
                foreach (var category in categoriesFound) {
                    categories.Add(category);
                }
            }
        }

        return categories;
    }

    /// <summary>
    ///     Finds registered assemblies that are associated with the specified categories.
    /// </summary>
    /// <param name="categories">The categories.</param>
    /// <returns>A set of assemblies associated with the specified categories.</returns>
    /// <exception cref="System.ArgumentNullException">categories</exception>
    public static HashSet<Assembly> Find(params string[] categories) => Find((IEnumerable<string>)categories);

    public static void RegisterScanTypes(Assembly assembly, ScanTypes types) {
        AssemblyToScanTypes.TryAdd(assembly, types);
    }

    public static ScanTypes? GetScanTypes(Assembly assembly) {
        AssemblyToScanTypes.TryGetValue(assembly, out var assemblyScanTypes);
        return assemblyScanTypes;
    }

    /// <summary>
    ///     Registers an assembly with the specified categories.
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    /// <param name="categories">The categories to associate with this assembly.</param>
    /// <exception cref="System.ArgumentNullException">
    ///     assembly
    ///     or
    ///     categories
    /// </exception>
    public static void Register(Assembly assembly, IEnumerable<string> categories) {
        if (assembly == null) {
            throw new ArgumentNullException(nameof(assembly));
        }

        if (categories == null) {
            throw new ArgumentNullException(nameof(categories));
        }

        HashSet<string> currentRegisteredCategories = new();

        lock (Lock) {
            if (!MapAssemblyToCategories.TryGetValue(assembly, out var registeredCategoriesPerAssembly)) {
                registeredCategoriesPerAssembly = new();
                MapAssemblyToCategories.Add(assembly, registeredCategoriesPerAssembly);
            }

            // Register the assembly name
            var assemblyName = assembly.GetName().Name!;
            AssemblyNameToAssembly[assemblyName] = assembly;

            foreach (var category in categories) {
                if (string.IsNullOrWhiteSpace(category)) {
                    Log.Error("Invalid empty category for assembly [{Assembly}]", assembly);
                    continue;
                }

                if (registeredCategoriesPerAssembly.Add(category)) {
                    currentRegisteredCategories.Add(category);
                }

                if (!MapCategoryToAssemblies.TryGetValue(category, out var registeredAssembliesPerCategory)) {
                    registeredAssembliesPerCategory = new();
                    MapCategoryToAssemblies.Add(category, registeredAssembliesPerCategory);
                }

                registeredAssembliesPerCategory.Add(assembly);
            }
        }

        if (currentRegisteredCategories.Count > 0) {
            OnAssemblyRegistered(assembly, currentRegisteredCategories);
        }
    }

    /// <summary>
    ///     Registers an assembly with the specified categories.
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    /// <param name="categories">The categories to associate with this assembly.</param>
    /// <exception cref="System.ArgumentNullException">
    ///     assembly
    ///     or
    ///     categories
    /// </exception>
    public static void Register(Assembly assembly, params string[] categories) {
        Register(assembly, (IEnumerable<string>)categories);
    }

    /// <summary>
    ///     Unregisters the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    public static void Unregister(Assembly assembly) {
        HashSet<string>? categoriesFound;

        lock (Lock) {
            if (MapAssemblyToCategories.TryGetValue(assembly, out categoriesFound)) {
                // Remove assembly => categories entry
                MapAssemblyToCategories.Remove(assembly);

                // Remove reverse category => assemblies entries
                foreach (var category in categoriesFound) {
                    if (MapCategoryToAssemblies.TryGetValue(category, out var assembliesFound)) {
                        assembliesFound.Remove(assembly);
                    }
                }
            }
        }

        if (categoriesFound != null) {
            OnAssemblyUnregistered(assembly, categoriesFound);
        }
    }

    static void OnAssemblyRegistered(Assembly assembly, HashSet<string> categories) {
        AssemblyRegistered?.Invoke(null, new(assembly, categories));
    }

    static void OnAssemblyUnregistered(Assembly assembly, HashSet<string> categories) {
        AssemblyUnregistered?.Invoke(null, new(assembly, categories));
    }

    /// <summary>
    ///     Occurs when an assembly is registered.
    /// </summary>
    public static event EventHandler<AssemblyRegisteredEventArgs> AssemblyRegistered;

    /// <summary>
    ///     Occurs when an assembly is registered.
    /// </summary>
    public static event EventHandler<AssemblyRegisteredEventArgs> AssemblyUnregistered;

    /// <summary>
    ///     List types that matches a given <see cref="AssemblyScanAttribute" /> for a given assembly.
    /// </summary>
    public class ScanTypes(Dictionary<Type, List<Type>> types) {
        public IReadOnlyDictionary<Type, List<Type>> Types { get; } = types;
    }
}
