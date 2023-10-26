namespace Rin.InputSystem;

/// <summary>
///     Provides information about a gamepad axis
/// </summary>
public class GameControllerAxisInfo : GameControllerObjectInfo {
    public override string ToString() => $"GameController Axis {{{Name}}}";
}
