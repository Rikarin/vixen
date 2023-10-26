using System.Collections.ObjectModel;

namespace Rin.InputSystem.VirtualButtons;

/// <summary>
///     A collection of <see cref="VirtualButtonConfig" />.
/// </summary>
/// <remarks>
///     Several virtual button configurations can be stored in this instance.
///     For example, User0 config could be stored on index 0, User1 on index 1...etc.
/// </remarks>
public class VirtualButtonConfigSet : Collection<VirtualButtonConfig> {
    /// <summary>
    ///     Initializes a new instance of the <see cref="VirtualButtonConfigSet" /> class.
    /// </summary>
    public VirtualButtonConfigSet() { }

    public virtual float GetValue(InputManager inputManager, int configIndex, object name) {
        if (configIndex < 0 || configIndex >= Count) {
            return 0.0f;
        }

        return this[configIndex]?.GetValue(inputManager, name) ?? 0.0f;
    }
}
