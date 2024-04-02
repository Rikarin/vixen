namespace Vixen.InputSystem;

/// <summary>
///     An event used when a device was changed
/// </summary>
public class DeviceChangedEventArgs : EventArgs {
    /// <summary>
    ///     The input source this device belongs to
    /// </summary>
    public IInputSource Source { get; init; }

    /// <summary>
    ///     The device that changed
    /// </summary>
    public IInputDevice Device { get; init; }

    /// <summary>
    ///     The type of change that happened
    /// </summary>
    public DeviceChangedEventType Type { get; init; }
}
