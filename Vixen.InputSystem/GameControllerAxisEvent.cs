namespace Vixen.InputSystem;

/// <summary>
///     An event to describe a change in a game controller axis state
/// </summary>
public class GameControllerAxisEvent : AxisEvent {
    /// <summary>
    ///     Index of the axis
    /// </summary>
    public int Index { get; internal set; }

    /// <summary>
    ///     The game controller that sent this event
    /// </summary>
    public IGameControllerDevice GameController => (IGameControllerDevice)Device;

    public override string ToString() =>
        $"{nameof(Index)}: {Index} ({GameController.AxisInfos[Index].Name}), {nameof(Value)}: {Value}, {nameof(GameController)}: {GameController.Name}";
}
