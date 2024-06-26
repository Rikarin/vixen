namespace Vixen.InputSystem;

/// <summary>
///     Provides easier ways to set vibration levels on a controller, rather than setting 4 motors
/// </summary>
public static class GamePadDeviceExtensions {
    /// <summary>
    ///     Sets all the gamepad vibration motors to the same amount
    /// </summary>
    /// <param name="gamepad">The gamepad</param>
    /// <param name="amount">The amount of vibration</param>
    public static void SetVibration(this IGamePadDevice gamepad, float amount) {
        gamepad.SetVibration(amount, amount, amount, amount);
    }

    /// <summary>
    ///     Sets the gamepad's large and small motors to the given amounts
    /// </summary>
    /// <param name="gamepad">The gamepad</param>
    /// <param name="largeMotors">The amount of vibration for the large motors</param>
    /// <param name="smallMotors">The amount of vibration for the small motors</param>
    public static void SetVibration(this IGamePadDevice gamepad, float largeMotors, float smallMotors) {
        gamepad.SetVibration(smallMotors, smallMotors, largeMotors, largeMotors);
    }

    /// <summary>
    ///     Determines whether the specified button is pressed since the previous update.
    /// </summary>
    /// <param name="gamepad">The gamepad</param>
    /// <param name="button">The button</param>
    /// <returns><c>true</c> if the specified button is pressed; otherwise, <c>false</c>.</returns>
    public static bool IsButtonPressed(this IGamePadDevice gamepad, GamePadButton button) =>
        gamepad.PressedButtons.Contains(button);

    /// <summary>
    ///     Determines whether the specified button is released since the previous update.
    /// </summary>
    /// ///
    /// <param name="gamepad">The gamepad</param>
    /// <param name="button">The button</param>
    /// <returns><c>true</c> if the specified button is released; otherwise, <c>false</c>.</returns>
    public static bool IsButtonReleased(this IGamePadDevice gamepad, GamePadButton button) =>
        gamepad.ReleasedButtons.Contains(button);

    /// <summary>
    ///     Determines whether the specified button is being pressed down
    /// </summary>
    /// ///
    /// <param name="gamepad">The gamepad</param>
    /// <param name="button">The button</param>
    /// <returns><c>true</c> if the specified button is being pressed down; otherwise, <c>false</c>.</returns>
    public static bool IsButtonDown(this IGamePadDevice gamepad, GamePadButton button) =>
        gamepad.DownButtons.Contains(button);
}
