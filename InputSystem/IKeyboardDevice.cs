namespace Rin.InputSystem;

/// <summary>
///     A keyboard device
/// </summary>
public interface IKeyboardDevice : IInputDevice {
    /// <summary>
    ///     The keys that have been pressed since the last frame
    /// </summary>
    IReadOnlySet<Key> PressedKeys { get; }

    /// <summary>
    ///     The keys that have been released since the last frame
    /// </summary>
    IReadOnlySet<Key> ReleasedKeys { get; }

    /// <summary>
    ///     List of keys that are currently down on this keyboard
    /// </summary>
    IReadOnlySet<Key> DownKeys { get; }
}
