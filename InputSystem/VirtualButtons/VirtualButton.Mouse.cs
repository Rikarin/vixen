namespace Rin.InputSystem.VirtualButtons;

/// <summary>
///     Describes a virtual button (a key from a keyboard, a mouse button, an axis of a joystick...etc.).
/// </summary>
public partial class VirtualButton {
    /// <summary>
    ///     Mouse virtual button.
    /// </summary>
    public class Mouse : VirtualButton {
        /// <summary>
        ///     Equivalent to <see cref="MouseButton.Left" />.
        /// </summary>
        public static readonly Mouse Left = new("Left", 0, false);

        /// <summary>
        ///     Equivalent to <see cref="MouseButton.Middle" />.
        /// </summary>
        public static readonly Mouse Middle = new("Middle", 1, false);

        /// <summary>
        ///     Equivalent to <see cref="MouseButton.Right" />.
        /// </summary>
        public static readonly Mouse Right = new("Right", 2, false);

        /// <summary>
        ///     Equivalent to <see cref="MouseButton.Extended1" />.
        /// </summary>
        public static readonly Mouse Extended1 = new("Extended1", 3, false);

        /// <summary>
        ///     Equivalent to <see cref="MouseButton.Extended2" />.
        /// </summary>
        public static readonly Mouse Extended2 = new("Extended2", 4, false);

        /// <summary>
        ///     Equivalent to X Axis of <see cref="InputManager.MousePosition" />.
        /// </summary>
        public static readonly Mouse PositionX = new("PositionX", 5, true);

        /// <summary>
        ///     Equivalent to Y Axis of <see cref="InputManager.MousePosition" />.
        /// </summary>
        public static readonly Mouse PositionY = new("PositionY", 6, true);

        /// <summary>
        ///     Equivalent to X Axis delta of <see cref="InputManager.MousePosition" />.
        /// </summary>
        public static readonly Mouse DeltaX = new("DeltaX", 7, true);

        /// <summary>
        ///     Equivalent to Y Axis delta of <see cref="InputManager.MousePosition" />.
        /// </summary>
        public static readonly Mouse DeltaY = new("DeltaY", 8, true);

        protected Mouse(string name, int id, bool isPositiveAndNegative)
            : base(name, VirtualButtonType.Mouse, id, isPositiveAndNegative) { }

        public override float GetValue(InputManager manager) {
            if (Index < 5) {
                if (IsDown(manager)) {
                    return 1.0f;
                }
            } else {
                switch (Index) {
                    case 5:
                        return manager.MousePosition.X;
                    case 6:
                        return manager.MousePosition.Y;
                    case 7:
                        return manager.MouseDelta.X;
                    case 8:
                        return manager.MouseDelta.Y;
                }
            }

            return 0.0f;
        }

        public override bool IsDown(InputManager manager) => Index < 5 && manager.IsMouseButtonDown((MouseButton)Index);

        public override bool IsPressed(InputManager manager) =>
            Index < 5 && manager.IsMouseButtonPressed((MouseButton)Index);

        public override bool IsReleased(InputManager manager) =>
            Index < 5 && manager.IsMouseButtonReleased((MouseButton)Index);
    }
}
