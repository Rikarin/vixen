namespace Rin.InputSystem.VirtualButtons;

/// <summary>
///     Type of a <see cref="VirtualButton" />.
/// </summary>
public enum VirtualButtonType {
    /// <summary>
    ///     A keyboard virtual button.
    /// </summary>
    Keyboard = 1 << 28,

    /// <summary>
    ///     A mouse virtual button.
    /// </summary>
    Mouse = 2 << 28,

    /// <summary>
    ///     A pointer virtual button.
    /// </summary>
    Pointer = 3 << 28,

    /// <summary>
    ///     A gamepad virtual button.
    /// </summary>
    GamePad = 4 << 28
}
