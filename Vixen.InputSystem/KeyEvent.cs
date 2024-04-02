namespace Vixen.InputSystem;

/// <summary>
///     Event for a keyboard button changing state
/// </summary>
public class KeyEvent : ButtonEvent {
    /// <summary>
    ///     The key that is being pressed or released.
    /// </summary>
    public Key Key { get; internal set; }

    /// <summary>
    ///     The repeat count for this key. If it is 0 this is the initial press of the key
    /// </summary>
    public int RepeatCount { get; internal set; }

    /// <summary>
    ///     The keyboard that sent this event
    /// </summary>
    public IKeyboardDevice Keyboard => (IKeyboardDevice)Device;

    public override string ToString() =>
        $"{nameof(Key)}: {Key}, {nameof(IsDown)}: {IsDown}, {nameof(RepeatCount)}: {RepeatCount}, {nameof(Keyboard)}: {Keyboard}";
}
