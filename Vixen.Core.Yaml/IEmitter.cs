using Vixen.Core.Yaml.Events;

namespace Vixen.Core.Yaml;

/// <summary>
///     Represents a YAML stream emitter.
/// </summary>
public interface IEmitter {
    /// <summary>
    ///     Emits an event.
    /// </summary>
    void Emit(ParsingEvent @event);
}
