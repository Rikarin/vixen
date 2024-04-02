namespace Vixen.InputSystem;

/// <summary>
///     An event to describe a change in game controller button state
/// </summary>
public class GameControllerButtonEvent : ButtonEvent {
    /// <summary>
    ///     The index of the button
    /// </summary>
    public int Index { get; internal set; }

    /// <summary>
    ///     The game controller that sent this event
    /// </summary>
    public IGameControllerDevice GameController => (IGameControllerDevice)Device;

    public override string ToString() =>
        $"{nameof(Index)}: {Index} ({GameController.ButtonInfos[Index].Name}), {nameof(IsDown)}: {IsDown}, {nameof(GameController)}: {GameController.Name}";
}
