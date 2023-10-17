namespace Rin.InputSystem;

/// <summary>
///     An event to describe a change in game controller direction state
/// </summary>
public class GameControllerDirectionEvent : InputEvent {
    /// <summary>
    ///     The index of the direction controller
    /// </summary>
    public int Index;

    /// <summary>
    ///     The new direction
    /// </summary>
    public Direction Direction;

    /// <summary>
    ///     The gamepad that sent this event
    /// </summary>
    public IGameControllerDevice GameController => (IGameControllerDevice)Device;

    public override string ToString() =>
        $"{nameof(Index)}: {Index} ({GameController.DirectionInfos[Index].Name}), {nameof(Direction)}: {Direction} ({GetDirectionFriendlyName()}), {nameof(GameController)}: {GameController.Name}";

    string GetDirectionFriendlyName() {
        if (Direction.IsNeutral) {
            return "Neutral";
        }

        return Direction.GetTicks(8) switch {
            0 => "Up",
            1 => "RightUp",
            2 => "Right",
            3 => "RightDown",
            4 => "Down",
            5 => "LeftDown",
            6 => "Left",
            7 => "LeftUp",
            _ => "Out of range"
        };
    }
}
