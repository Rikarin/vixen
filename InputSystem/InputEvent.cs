namespace Rin.InputSystem;

/// <summary>
///     An event that was generated from an <see cref="IInputDevice" />
/// </summary>
public abstract class InputEvent : IInputEventArgs {
    /// <inheritdoc />
    public IInputDevice Device { get; protected internal set; }
}
