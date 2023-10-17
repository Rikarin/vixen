namespace Rin.InputSystem;

/// <summary>
///     An event to describe a change in gamepad button state
/// </summary>
public class GamePadButtonEvent : ButtonEvent {
    /// <summary>
    ///     The gamepad button identifier
    /// </summary>
    public GamePadButton Button { get; internal set; }

    /// <summary>
    ///     The gamepad that sent this event
    /// </summary>
    public IGamePadDevice GamePad => (IGamePadDevice)Device;

    public override string ToString() =>
        $"{nameof(Button)}: {Button}, {nameof(IsDown)}: {IsDown}, {nameof(GamePad)}: {GamePad.Name}";
}
