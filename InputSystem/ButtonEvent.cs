namespace Rin.InputSystem;

/// <summary>
///     Event for a button changing state on a device
/// </summary>
public abstract class ButtonEvent : InputEvent {
    /// <summary>
    ///     The new state of the button
    /// </summary>
    public bool IsDown { get; internal set; }
}
