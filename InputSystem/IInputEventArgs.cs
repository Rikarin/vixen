namespace Rin.InputSystem;

public interface IInputEventArgs {
    /// <summary>
    ///     The device that sent this event
    /// </summary>
    IInputDevice Device { get; }
}
