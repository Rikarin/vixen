namespace Vixen.InputSystem;

/// <summary>
///     Event for an axis changing state on a device
/// </summary>
public abstract class AxisEvent : InputEvent {
    /// <summary>
    ///     The new value of the axis
    /// </summary>
    public float Value { get; internal set; }
}
