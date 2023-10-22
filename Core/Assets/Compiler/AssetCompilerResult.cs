namespace Rin.Core.Assets.Compiler;

/// <summary>
///     Result of a compilation of assets when using <see cref="IAssetCompiler.Prepare" />
/// </summary>
public class AssetCompilerResult {
    /// <summary>
    ///     Gets or sets the build steps generated for the build engine.
    /// </summary>
    /// <value>The build step.</value>
    public ListBuildStep BuildSteps { get; set; } = new();
}
