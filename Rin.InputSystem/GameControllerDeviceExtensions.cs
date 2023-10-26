namespace Rin.InputSystem;

/// <summary>
///     Provides easier ways to set vibration levels on a controller, rather than setting 4 motors
/// </summary>
public static class GameControllerDeviceExtensions {
    /// <summary>
    ///     Determines whether the specified button is pressed since the previous update.
    /// </summary>
    /// <param name="controller">The controller</param>
    /// <param name="button">The button</param>
    /// <returns><c>true</c> if the specified button is pressed; otherwise, <c>false</c>.</returns>
    public static bool IsButtonPressed(this IGameControllerDevice controller, int button) =>
        controller.PressedButtons.Contains(button);

    /// <summary>
    ///     Determines whether the specified button is released since the previous update.
    /// </summary>
    /// ///
    /// <param name="controller">The controller</param>
    /// <param name="button">The button</param>
    /// <returns><c>true</c> if the specified button is released; otherwise, <c>false</c>.</returns>
    public static bool IsButtonReleased(this IGameControllerDevice controller, int button) =>
        controller.ReleasedButtons.Contains(button);

    /// <summary>
    ///     Determines whether the specified button is being pressed down
    /// </summary>
    /// ///
    /// <param name="controller">The controller</param>
    /// <param name="button">The button</param>
    /// <returns><c>true</c> if the specified button is being pressed down; otherwise, <c>false</c>.</returns>
    public static bool IsButtonDown(this IGameControllerDevice controller, int button) =>
        controller.DownButtons.Contains(button);
}
