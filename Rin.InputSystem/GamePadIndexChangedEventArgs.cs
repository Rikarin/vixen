namespace Rin.InputSystem;

/// <summary>
///     Event for when a <see cref="IGamePadDevice" />'s index changed
/// </summary>
public class GamePadIndexChangedEventArgs : EventArgs {
    /// <summary>
    ///     New device index
    /// </summary>
    public int Index { get; internal set; }

    /// <summary>
    ///     if <c>true</c>, this change was initiate by the device
    /// </summary>
    public bool IsDeviceSideChange { get; internal set; }
}
