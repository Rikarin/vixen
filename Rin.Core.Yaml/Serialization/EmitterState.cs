namespace Rin.Core.Yaml.Serialization;

/// <summary>
///     Holds state that is used when emitting a stream.
/// </summary>
class EmitterState {
    /// <summary>
    ///     Gets the already emitted anchors.
    /// </summary>
    /// <value>The emitted anchors.</value>
    public HashSet<string> EmittedAnchors { get; } = [];
}
