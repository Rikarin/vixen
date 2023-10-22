namespace Rin.Core.Assets.Analysis;

[Flags]
public enum BuildDependencyType {
    /// <summary>
    ///     The content generated during compilation needs the content compiled from the target asset to be loaded at runtime.
    /// </summary>
    Runtime = 0x1,

    /// <summary>
    ///     The un-compiled target asset is accessed during compilation.
    /// </summary>
    CompileAsset = 0x2,

    /// <summary>
    ///     The content compiled from the target asset is needed during compilation.
    /// </summary>
    CompileContent = 0x4
}
