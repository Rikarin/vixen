namespace Rin.InputSystem;

/// <summary>
///     Describes a button on a mouse changing state
/// </summary>
public class MouseButtonEvent : ButtonEvent {
    /// <summary>
    ///     The button that changed state
    /// </summary>
    public MouseButton Button { get; internal set; }

    /// <summary>
    ///     The mouse that sent this event
    /// </summary>
    public IMouseDevice Mouse => (IMouseDevice)Device;

    public override string ToString() =>
        $"{nameof(Button)}: {Button}, {nameof(IsDown)}: {IsDown}, {nameof(Mouse)}: {Mouse.Name}";
}
