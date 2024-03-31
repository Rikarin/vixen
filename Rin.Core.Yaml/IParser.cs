using Rin.Core.Yaml.Events;

namespace Rin.Core.Yaml;

/// <summary>
///     Represents a YAML stream paser.
/// </summary>
public interface IParser {
    /// <summary>
    ///     Gets the current event.
    /// </summary>
    ParsingEvent Current { get; }

    /// <summary>
    ///     True if end of stream has been reached, false otherwise.
    /// </summary>
    bool IsEndOfStream { get; }

    /// <summary>
    ///     Moves to the next event.
    /// </summary>
    /// <returns>Returns true if there are more events available, otherwise returns false.</returns>
    bool MoveNext();
}
