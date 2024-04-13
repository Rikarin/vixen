using Serilog;
using System.Diagnostics;
using System.Reflection;
using Vixen.Core.Reflection;

namespace Vixen.Core.Assets.Compiler;

public sealed class AssetCompilerRegistry {
    readonly HashSet<Assembly> assembliesToRegister = new();
    readonly ILogger log = Log.ForContext<AssetCompilerRegistry>();

    readonly HashSet<Assembly> registeredAssemblies = new();
    readonly Dictionary<CompilerTypeData, IAssetCompiler> typeToCompiler = new();
    bool assembliesChanged;

    /// <summary>
    ///     Gets or sets the default compiler to use when no compiler are explicitly registered for a type.
    /// </summary>
    public IAssetCompiler DefaultCompiler { get; set; }


    /// <summary>
    ///     Create an instance of that registry
    /// </summary>
    public AssetCompilerRegistry() {
        // Statically find all assemblies related to assets and register them
        var assemblies = AssemblyRegistry.Find(AssemblyCommonCategories.Assets);
        foreach (var assembly in assemblies) {
            RegisterAssembly(assembly);
        }

        AssemblyRegistry.AssemblyRegistered += AssemblyRegistered;
        AssemblyRegistry.AssemblyUnregistered += AssemblyUnregistered;
    }


    /// <summary>
    ///     Gets the compiler associated to an <see cref="Asset" /> type.
    /// </summary>
    /// <param name="type">The type of the <see cref="Asset" /></param>
    /// <param name="context"></param>
    /// <returns>The compiler associated the provided asset type or null if no compiler exists for that type.</returns>
    public IAssetCompiler GetCompiler(Type type, Type context) {
        AssertAssetType(type);
        EnsureTypes();

        var typeData = new CompilerTypeData(context, type);

        if (!typeToCompiler.TryGetValue(typeData, out var compiler)) {
            if (context.BaseType != typeof(object)) {
                return GetCompiler(type, context.BaseType!);
            }

            compiler = DefaultCompiler;
        }

        return compiler;
    }

    /// <summary>
    ///     Register a compiler for a given <see cref="Asset" /> type.
    /// </summary>
    /// <param name="type">The type of asset the compiler can compile</param>
    /// <param name="compiler">The compiler to use</param>
    /// <param name="context"></param>
    void RegisterCompiler(Type type, IAssetCompiler compiler, Type context) {
        AssertAssetType(type);
        var typeData = new CompilerTypeData(context, type);
        typeToCompiler[typeData] = compiler;
    }

    void UnregisterCompilersFromAssembly(Assembly assembly) {
        foreach (var typeToRemove in typeToCompiler
                     .Where(
                         typeAndCompile =>
                             typeAndCompile.Key.Type.Assembly == assembly
                             || typeAndCompile.Value.GetType().Assembly == assembly
                     )
                     .Select(e => e.Key)
                     .ToList()) {
            typeToCompiler.Remove(typeToRemove);
        }
    }

    static void AssertAssetType(Type assetType) {
        if (assetType == null) {
            throw new ArgumentNullException(nameof(assetType));
        }

        if (!typeof(Asset).IsAssignableFrom(assetType)) {
            throw new ArgumentException($"Type [{assetType}] must be assignable to Asset", nameof(assetType));
        }
    }

    void AssemblyRegistered(object? sender, AssemblyRegisteredEventArgs e) {
        // Handle delay-loading assemblies
        if (e.Categories.Contains(AssemblyCommonCategories.Assets)) {
            RegisterAssembly(e.Assembly);
        }
    }

    void AssemblyUnregistered(object? sender, AssemblyRegisteredEventArgs e) {
        if (e.Categories.Contains(AssemblyCommonCategories.Assets)) {
            UnregisterAssembly(e.Assembly);
        }
    }

    void EnsureTypes() {
        if (assembliesChanged) {
            Assembly[] assembliesToRegisterCopy;
            lock (assembliesToRegister) {
                assembliesToRegisterCopy = assembliesToRegister.ToArray();
                assembliesToRegister.Clear();
                assembliesChanged = false;
            }

            foreach (var assembly in assembliesToRegisterCopy) {
                if (!registeredAssemblies.Contains(assembly)) {
                    RegisterCompilersFromAssembly(assembly);
                    registeredAssemblies.Add(assembly);
                }
            }
        }
    }

    void ProcessAttribute(AssetCompilerAttribute compilerCompilerAttribute, Type type) {
        if (!typeof(ICompilationContext).IsAssignableFrom(compilerCompilerAttribute.CompilationContext)) {
            log.Error(
                "Invalid compiler context type [{CompilationContext}], must inherit from ICompilerContext",
                compilerCompilerAttribute.CompilationContext
            );
            return;
        }

        var assetType = AssemblyRegistry.GetType(compilerCompilerAttribute.TypeName);
        if (assetType == null) {
            log.Error(
                "Unable to find asset [{TypeName}] for compiler [{Type}]",
                compilerCompilerAttribute.TypeName,
                type
            );
            return;
        }

        var compilerInstance = Activator.CreateInstance(type) as IAssetCompiler;
        if (compilerInstance == null) {
            log.Error("Invalid compiler type [{Type}], must inherit from IAssetCompiler", type);
            return;
        }

        RegisterCompiler(assetType, compilerInstance, compilerCompilerAttribute.CompilationContext);
    }

    void RegisterAssembly(Assembly assembly) {
        lock (assembliesToRegister) {
            assembliesToRegister.Add(assembly);
            assembliesChanged = true;
        }
    }

    void RegisterCompilersFromAssembly(Assembly assembly) {
        // Process Asset types.
        foreach (var type in GetFullyLoadedTypes(assembly)) {
            // Only process Asset types
            if (!typeof(IAssetCompiler).IsAssignableFrom(type) || !type.IsClass) {
                continue;
            }

            // Asset compiler
            var compilerAttribute = type.GetCustomAttribute<AssetCompilerAttribute>();
            if (compilerAttribute == null) {
                continue;
            }

            try {
                ProcessAttribute(compilerAttribute, type);
            } catch (Exception ex) {
                log.Error(
                    ex,
                    "Unable to instantiate compiler [{CompilerAttributeTypeName}]",
                    compilerAttribute.TypeName
                );
            }
        }

        // Taken from https://stackoverflow.com/questions/7889228/how-to-prevent-reflectiontypeloadexception-when-calling-assembly-gettypes
        [DebuggerNonUserCode]
        IEnumerable<Type> GetFullyLoadedTypes(Assembly assembly) {
            try {
                return assembly.GetTypes();
            } catch (ReflectionTypeLoadException ex) {
                log.Warning($"Could not load all types from assembly {assembly.FullName}", ex);
                return ex.Types.Where(t => t != null);
            }
        }
    }

    void UnregisterAssembly(Assembly assembly) {
        registeredAssemblies.Remove(assembly);
        UnregisterCompilersFromAssembly(assembly);
        assembliesChanged = true;
    }

    readonly record struct CompilerTypeData(Type Context, Type Type);
}
