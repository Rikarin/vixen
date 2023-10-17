using System.Collections.ObjectModel;
using System.Text;

namespace Rin.InputSystem.VirtualButtons;

/// <summary>
///     A combination <see cref="IVirtualButton" />, by default evaluated as the operator '&amp;&amp;' to produce a value
///     if all buttons are pressed.
/// </summary>
public class VirtualButtonGroup : Collection<IVirtualButton>, IVirtualButton {
    /// <summary>
    ///     Gets or sets a value indicating whether this instance is determining the value as a disjunction ('&#124;&#124;'
    ///     operator between buttons), false by default ('&amp;&amp;' operator).
    /// </summary>
    /// <value><c>true</c> if this instance is disjunction; otherwise, <c>false</c>.</value>
    public bool IsDisjunction { get; set; }

    public VirtualButtonGroup(bool isDisjunction = false) {
        IsDisjunction = isDisjunction;
    }

    /// <summary>
    ///     Implements the + operator.
    /// </summary>
    /// <param name="combination">The combination.</param>
    /// <param name="virtualButton">The virtual button.</param>
    /// <returns>The result of the operator.</returns>
    public static VirtualButtonGroup operator +(VirtualButtonGroup combination, IVirtualButton virtualButton) {
        combination.Add(virtualButton);
        return combination;
    }

    public virtual float GetValue(InputManager manager) {
        var value = 0.0f;
        foreach (var virtualButton in Items) {
            var newValue = virtualButton != null ? virtualButton.GetValue(manager) : 0.0f;

            // In case of a || (disjunction) set, we return the latest non-zero value.
            if (IsDisjunction) {
                if (newValue != 0.0f) {
                    value = newValue;
                }
            } else {
                // In case of a && (conjunction) set, we return the last non-zero value unless there is a zero value.
                if (newValue == 0.0f) {
                    return 0.0f;
                }

                value = newValue;
            }
        }

        return value;
    }

    public override string ToString() {
        var text = new StringBuilder();
        for (var i = 0; i < Items.Count; i++) {
            var virtualButton = Items[i];
            if (i > 0) {
                text.Append(IsDisjunction ? " || " : " && ");
            }

            text.AppendFormat("{0}", virtualButton);
        }

        return text.ToString();
    }

    public bool IsDown(InputManager manager) => CheckAnyOrAll(manager, IsDown);
    public bool IsPressed(InputManager manager) => CheckAnyOrAll(manager, IsPressed);
    public bool IsReleased(InputManager manager) => CheckAnyOrAll(manager, IsReleased);
    bool IsDown(IVirtualButton button, InputManager manager) => button.IsDown(manager);
    bool IsReleased(IVirtualButton button, InputManager manager) => button.IsReleased(manager);
    bool IsPressed(IVirtualButton button, InputManager manager) => button.IsPressed(manager);

    bool CheckAnyOrAll(InputManager manager, Func<IVirtualButton, InputManager, bool> check) {
        foreach (var virtualButton in Items) {
            var isDown = check(virtualButton, manager);

            if (IsDisjunction && isDown) {
                return true;
            }

            if (!IsDisjunction && !isDown) {
                return false;
            }
        }

        return !IsDisjunction;
    }

    protected override void InsertItem(int index, IVirtualButton item) {
        if (item == null) {
            throw new ArgumentNullException(nameof(item), "Cannot set null instance of VirtualButton");
        }

        if (!Contains(item)) {
            base.InsertItem(index, item);
        }
    }

    protected override void SetItem(int index, IVirtualButton item) {
        if (item == null) {
            throw new ArgumentNullException(nameof(item), "Cannot add null instance of VirtualButton");
        }

        if (!Contains(item)) {
            base.SetItem(index, item);
        }
    }
}
