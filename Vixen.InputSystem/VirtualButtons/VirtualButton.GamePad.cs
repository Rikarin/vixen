using Vixen.Core.Common.Mathematics;

namespace Vixen.InputSystem.VirtualButtons;

/// <summary>
///     Describes a virtual button (a key from a keyboard, a mouse button, an axis of a joystick...etc.).
/// </summary>
public partial class VirtualButton {
    /// <summary>
    ///     GamePad virtual button.
    /// </summary>
    public class GamePad : VirtualButton {
        /// <summary>
        ///     Equivalent to <see cref="GamePadButton.PadUp" />.
        /// </summary>
        public static readonly GamePad PadUp = new("PadUp", 0);

        /// <summary>
        ///     Equivalent to <see cref="GamePadButton.PadDown" />.
        /// </summary>
        public static readonly GamePad PadDown = new("PadDown", 1);

        /// <summary>
        ///     Equivalent to <see cref="GamePadButton.PadLeft" />.
        /// </summary>
        public static readonly GamePad PadLeft = new("PadLeft", 2);

        /// <summary>
        ///     Equivalent to <see cref="GamePadButton.PadRight" />.
        /// </summary>
        public static readonly GamePad PadRight = new("PadRight", 3);

        /// <summary>
        ///     Equivalent to <see cref="GamePadButton.Start" />.
        /// </summary>
        public static readonly GamePad Start = new("Start", 4);

        /// <summary>
        ///     Equivalent to <see cref="GamePadButton.Back" />.
        /// </summary>
        public static readonly GamePad Back = new("Back", 5);

        /// <summary>
        ///     Equivalent to <see cref="GamePadButton.LeftThumb" />.
        /// </summary>
        public static readonly GamePad LeftThumb = new("LeftThumb", 6);

        /// <summary>
        ///     Equivalent to <see cref="GamePadButton.RightThumb" />.
        /// </summary>
        public static readonly GamePad RightThumb = new("RightThumb", 7);

        /// <summary>
        ///     Equivalent to <see cref="GamePadButton.LeftShoulder" />.
        /// </summary>
        public static readonly GamePad LeftShoulder = new("LeftShoulder", 8);

        /// <summary>
        ///     Equivalent to <see cref="GamePadButton.RightShoulder" />.
        /// </summary>
        public static readonly GamePad RightShoulder = new("RightShoulder", 9);

        /// <summary>
        ///     Equivalent to <see cref="GamePadButton.A" />.
        /// </summary>
        public static readonly GamePad A = new("A", 12);

        /// <summary>
        ///     Equivalent to <see cref="GamePadButton.B" />.
        /// </summary>
        public static readonly GamePad B = new("B", 13);

        /// <summary>
        ///     Equivalent to <see cref="GamePadButton.X" />.
        /// </summary>
        public static readonly GamePad X = new("X", 14);

        /// <summary>
        ///     Equivalent to <see cref="GamePadButton.Y" />.
        /// </summary>
        public static readonly GamePad Y = new("Y", 15);

        /// <summary>
        ///     Equivalent to the X Axis of <see cref="GamePadState.LeftThumb" />.
        /// </summary>
        public static readonly GamePad LeftThumbAxisX = new("LeftThumbAxisX", 16, true);

        /// <summary>
        ///     Equivalent to the Y Axis of <see cref="GamePadState.LeftThumb" />.
        /// </summary>
        public static readonly GamePad LeftThumbAxisY = new("LeftThumbAxisY", 17, true);

        /// <summary>
        ///     Equivalent to the X Axis of <see cref="GamePadState.RightThumb" />.
        /// </summary>
        public static readonly GamePad RightThumbAxisX = new("RightThumbAxisX", 18, true);

        /// <summary>
        ///     Equivalent to the Y Axis of <see cref="GamePadState.RightThumb" />.
        /// </summary>
        public static readonly GamePad RightThumbAxisY = new("RightThumbAxisY", 19, true);

        /// <summary>
        ///     Equivalent to <see cref="GamePadState.LeftTrigger" />.
        /// </summary>
        public static readonly GamePad LeftTrigger = new("LeftTrigger", 20);

        /// <summary>
        ///     Equivalent to <see cref="GamePadState.RightTrigger" />.
        /// </summary>
        public static readonly GamePad RightTrigger = new("RightTrigger", 21);

        /// <summary>
        ///     The pad index.
        /// </summary>
        public int PadIndex { get; }

        GamePad(string name, int id, bool isPositiveAndNegative = false)
            : this(name, id, -1, isPositiveAndNegative) { }

        GamePad(GamePad parentPad, int index)
            : this(parentPad.ShortName, parentPad.Id, index, parentPad.IsPositiveAndNegative) { }

        protected GamePad(string name, int id, int padIndex, bool isPositiveAndNegative)
            : base(name, VirtualButtonType.GamePad, id, isPositiveAndNegative) {
            PadIndex = padIndex;
        }

        /// <summary>
        ///     Return an instance of a particular GamePad.
        /// </summary>
        /// <param name="index">The gamepad index.</param>
        /// <returns>A new GamePad button linked to the gamepad index.</returns>
        public GamePad OfGamePad(int index) => new(this, index);

        public override float GetValue(InputManager manager) {
            var gamePad = GetGamePad(manager);
            if (gamePad == null) {
                return 0.0f;
            }

            if (Index <= 15) {
                if (IsDown(manager)) {
                    return 1.0f;
                }
            } else {
                var state = gamePad.State;
                switch (Index) {
                    case 16:
                        return state.LeftThumb.X;
                    case 17:
                        return state.LeftThumb.Y;
                    case 18:
                        return state.RightThumb.X;
                    case 19:
                        return state.RightThumb.Y;
                    case 20:
                        return state.LeftTrigger;
                    case 21:
                        return state.RightTrigger;
                }
            }

            return 0.0f;
        }

        public override bool IsDown(InputManager manager) {
            var gamePad = GetGamePad(manager);
            if (gamePad == null) {
                return false;
            }

            if (Index <= 15) {
                return gamePad.IsButtonDown((GamePadButton)(1 << Index));
            }

            if (Index == 20) {
                return gamePad.State.LeftTrigger > 1f - MathUtils.ZeroTolerance;
            }

            if (Index == 21) {
                return gamePad.State.RightTrigger > 1f - MathUtils.ZeroTolerance;
            }

            return false;
        }

        public override bool IsPressed(InputManager manager) {
            var gamePad = GetGamePad(manager);
            if (gamePad == null) {
                return false;
            }

            if (Index > 15) {
                return false;
            }

            return gamePad.IsButtonPressed((GamePadButton)(1 << Index));
        }

        public override bool IsReleased(InputManager manager) {
            var gamePad = GetGamePad(manager);
            if (gamePad == null) {
                return false;
            }

            if (Index > 15) {
                return false;
            }

            return gamePad.IsButtonReleased((GamePadButton)(1 << Index));
        }

        IGamePadDevice? GetGamePad(InputManager manager) =>
            PadIndex >= 0 ? manager.GetGamePadByIndex(PadIndex) : manager.DefaultGamePad;

        protected override string BuildButtonName() =>
            PadIndex < 0 ? base.BuildButtonName() : Type.ToString() + PadIndex + "." + ShortName;
    }
}
