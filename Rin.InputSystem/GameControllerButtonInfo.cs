namespace Rin.InputSystem;

/// <summary>
///     Provides information about a gamepad button
/// </summary>
public class GameControllerButtonInfo : GameControllerObjectInfo {
    /// <summary>
    ///     The type of button
    /// </summary>
    public GameControllerButtonType Type { get; internal set; }

    public override string ToString() => $"GameController Button {{{Name}}} [{Type}]";
}
