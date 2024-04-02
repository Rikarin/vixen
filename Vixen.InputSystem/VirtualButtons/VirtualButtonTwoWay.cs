namespace Vixen.InputSystem.VirtualButtons;

/// <summary>
///     Describes a <see cref="IVirtualButton" /> using the sum of a negative and positive button.
/// </summary>
/// <remarks>
///     This virtual button is for example useful to bind a key to a negative value (key 'left')
///     and another key to a positive value (key 'right') thus simulating an axis button.
///     The result of this virtual
///     <code>Result = PositiveButton.GetValue - NegativeButton.GetValue;</code>
/// </remarks>
public class VirtualButtonTwoWay : IVirtualButton {
    /// <summary>
    ///     Gets or sets the negative button that will generate a 'negative' value if it is pressed.
    /// </summary>
    /// <value>The negative button.</value>
    public IVirtualButton NegativeButton { get; set; }

    /// <summary>
    ///     Gets or sets the positive button that will generate a 'positive' value if it is pressed.
    /// </summary>
    /// <value>The positive button.</value>
    public IVirtualButton PositiveButton { get; set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="VirtualButtonTwoWay" /> class.
    /// </summary>
    public VirtualButtonTwoWay() : this(new VirtualButtonGroup(), new VirtualButtonGroup()) { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="VirtualButtonTwoWay" /> class.
    /// </summary>
    /// <param name="negativeButton">The negative button.</param>
    /// <param name="positiveButton">The positive button.</param>
    public VirtualButtonTwoWay(IVirtualButton negativeButton, IVirtualButton positiveButton) {
        NegativeButton = negativeButton;
        PositiveButton = positiveButton;
    }

    public virtual float GetValue(InputManager manager) {
        var negativeValue = NegativeButton.GetValue(manager);
        var positiveValue = PositiveButton.GetValue(manager);
        return positiveValue - negativeValue;
    }

    public bool IsDown(InputManager manager) => false;
    public bool IsPressed(InputManager manager) => false;
    public bool IsReleased(InputManager manager) => false;

    public override string ToString() => $"<{NegativeButton} , {PositiveButton}>";
}
