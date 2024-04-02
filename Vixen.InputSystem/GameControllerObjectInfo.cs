namespace Vixen.InputSystem;

/// <summary>
///     Provides information about an object exposed by a gamepad
/// </summary>
public class GameControllerObjectInfo {
    /// <summary>
    ///     The name of the object, reported by the device
    /// </summary>
    public string Name { get; init; }

    public override string ToString() => $"GameController Object {{{Name}}}";
}
