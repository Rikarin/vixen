namespace Rin.InputSystem;

/// <summary>
///     Arguments for input pre update event
/// </summary>
public class InputPreUpdateEventArgs : EventArgs {
    /// <summary>
    ///     The game time passed to <see cref="InputManager.Update" />
    /// </summary>
    public float DeltaTime { get; init; }
    // public GameTime GameTime;
}
