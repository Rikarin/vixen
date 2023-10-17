namespace Rin.InputSystem;

/// <summary>
///     Input event used for text input and IME related events
/// </summary>
public class TextInputEvent : InputEvent {
    /// <summary>
    ///     The text that was entered
    /// </summary>
    public string Text { get; }

    /// <summary>
    ///     The type of text input event
    /// </summary>
    public TextInputEventType Type { get; }

    /// <summary>
    ///     Start of the current composition being edited
    /// </summary>
    public int CompositionStart { get; }

    /// <summary>
    ///     Length of the current part of the composition being edited
    /// </summary>
    public int CompositionLength { get; }

    public override string ToString() =>
        $"{nameof(Text)}: {Text}, {nameof(Type)}: {Type}, {nameof(CompositionStart)}: {CompositionStart}, {nameof(CompositionLength)}: {CompositionLength}";
}
