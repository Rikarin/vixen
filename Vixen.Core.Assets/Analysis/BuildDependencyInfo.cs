using Vixen.Core.Assets.Compiler;

namespace Vixen.Core.Assets.Analysis;

public readonly record struct BuildDependencyInfo {
    /// <summary>
    ///     The compilation context in which to compile the target asset.
    /// </summary>
    /// <remarks>
    ///     This context is not relevant if the asset is not compiled, like when <see cref="DependencyType" /> is
    ///     <see cref="BuildDependencyType.CompileAsset" />
    /// </remarks>
    public Type CompilationContext { get; }

    /// <summary>
    ///     The type of asset targeted by this dependency.
    /// </summary>
    public Type AssetType { get; }

    /// <summary>
    ///     The type of dependency, indicating whether the target asset must actually be compiled, and whether it should be
    ///     compiled before the referencing asset or can be at the same time.
    /// </summary>
    public BuildDependencyType DependencyType { get; }

    public BuildDependencyInfo(Type assetType, Type compilationContext, BuildDependencyType dependencyType) {
        if (!typeof(Asset).IsAssignableFrom(assetType)) {
            throw new ArgumentException($@"{nameof(assetType)} should inherit from Asset", nameof(assetType));
        }

        if (!typeof(ICompilationContext).IsAssignableFrom(compilationContext)) {
            throw new ArgumentException(
                $"{nameof(compilationContext)} should inherit from ICompilationContext",
                nameof(compilationContext)
            );
        }

        AssetType = assetType;
        CompilationContext = compilationContext;
        DependencyType = dependencyType;
    }
}
