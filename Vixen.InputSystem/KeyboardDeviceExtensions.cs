namespace Vixen.InputSystem;

public static class KeyboardDeviceExtensions {
    /// <summary>
    ///     Determines whether the specified key is pressed since the previous update.
    /// </summary>
    /// <param name="keyboardDevice">The keyboard</param>
    /// <param name="key">The key</param>
    /// <returns><c>true</c> if the specified key is pressed; otherwise, <c>false</c>.</returns>
    public static bool IsKeyPressed(this IKeyboardDevice keyboardDevice, Key key) =>
        keyboardDevice.PressedKeys.Contains(key);

    /// <summary>
    ///     Determines whether the specified key is released since the previous update.
    /// </summary>
    /// <param name="keyboardDevice">The keyboard</param>
    /// <param name="key">The key</param>
    /// <returns><c>true</c> if the specified key is released; otherwise, <c>false</c>.</returns>
    public static bool IsKeyReleased(this IKeyboardDevice keyboardDevice, Key key) =>
        keyboardDevice.ReleasedKeys.Contains(key);

    /// <summary>
    ///     Determines whether the specified key is being pressed down
    /// </summary>
    /// <param name="keyboardDevice">The keyboard</param>
    /// <param name="key">The key</param>
    /// <returns><c>true</c> if the specified key is being pressed down; otherwise, <c>false</c>.</returns>
    public static bool IsKeyDown(this IKeyboardDevice keyboardDevice, Key key) => keyboardDevice.DownKeys.Contains(key);
}
