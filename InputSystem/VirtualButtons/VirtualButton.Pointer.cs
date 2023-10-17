namespace Rin.InputSystem.VirtualButtons;

/// <summary>
///     Describes a virtual button (a key from a keyboard, a mouse button, an axis of a joystick...etc.).
/// </summary>
public partial class VirtualButton {
    /// <summary>
    ///     Mouse virtual button.
    /// </summary>
    public class Pointer : VirtualButton {
        /// <summary>
        ///     The pad index.
        /// </summary>
        public readonly int PointerId;

        /// <summary>
        ///     The current state of pointers.
        /// </summary>
        public static readonly Pointer State = new("State", 0, false);

        /// <summary>
        ///     The X component of the Pointer <see cref="PointerPoint.Position" />.
        /// </summary>
        public static readonly Pointer PositionX = new("PositionX", 1, true);

        /// <summary>
        ///     The Y component of the Pointer <see cref="PointerPoint.Position" />.
        /// </summary>
        public static readonly Pointer PositionY = new("PositionY", 2, true);

        /// <summary>
        ///     The X component of the Pointer <see cref="PointerPoint.Delta" />.
        /// </summary>
        public static readonly Pointer DeltaX = new("DeltaX", 3, true);

        /// <summary>
        ///     The Y component of the Pointer <see cref="PointerPoint.Delta" />.
        /// </summary>
        public static readonly Pointer DeltaY = new("DeltaY", 4, true);

        Pointer(string name, int id, bool isPositiveAndNegative)
            : this(name, id, -1, isPositiveAndNegative) { }

        Pointer(Pointer parent, int pointerId)
            : this(parent.ShortName, parent.Id, pointerId, parent.IsPositiveAndNegative) { }

        protected Pointer(string name, int id, int pointerId, bool isPositiveAndNegative)
            : base(name, VirtualButtonType.Pointer, id, isPositiveAndNegative) {
            PointerId = pointerId;
        }

        /// <summary>
        ///     Return a pointer button for the given point Id.
        /// </summary>
        /// <param name="pointerId">the Id of the pointer</param>
        /// <returns>An pointer button for the given pointer Id.</returns>
        public Pointer WithId(int pointerId) => new(this, pointerId);

        public override float GetValue(InputManager manager) {
            var index = Id & TypeIdMask;
            return index switch {
                0 => IsDown(manager) ? 1f : 0f,
                1 => FromFirstMatchingEvent(manager, GetPositionX),
                2 => FromFirstMatchingEvent(manager, GetPositionY),
                3 => FromFirstMatchingEvent(manager, GetDeltaX),
                4 => FromFirstMatchingEvent(manager, GetDeltaY),
                _ => 0.0f
            };
        }

        public override bool IsDown(InputManager manager) => Index == 0 && AnyPointerInState(manager, GetDownPointers);

        public override bool IsPressed(InputManager manager) =>
            Index == 0 && AnyPointerInState(manager, GetPressedPointers);

        public override bool IsReleased(InputManager manager) =>
            Index == 0 && AnyPointerInState(manager, GetReleasedPointers);

        float FromFirstMatchingEvent(InputManager manager, Func<PointerEvent, float> valueGetter) {
            foreach (var pointerEvent in manager.PointerEvents) {
                if (PointerId < 0 || pointerEvent.PointerId == PointerId) {
                    return valueGetter(pointerEvent);
                }
            }

            return 0f;
        }

        bool AnyPointerInState(
            InputManager manager,
            Func<IPointerDevice, IReadOnlySet<PointerPoint>> stateGetter
        ) =>
            manager.Pointers.Any(
                pointerDevice => stateGetter(pointerDevice)
                    .Any(pointerPoint => PointerId < 0 || pointerPoint.Id == PointerId)
            );

        IReadOnlySet<PointerPoint> GetDownPointers(IPointerDevice device) => device.DownPointers;
        IReadOnlySet<PointerPoint> GetPressedPointers(IPointerDevice device) => device.DownPointers;
        IReadOnlySet<PointerPoint> GetReleasedPointers(IPointerDevice device) => device.DownPointers;

        float GetPositionX(PointerEvent pointerEvent) => pointerEvent.Position.X;
        float GetPositionY(PointerEvent pointerEvent) => pointerEvent.Position.Y;
        float GetDeltaX(PointerEvent pointerEvent) => pointerEvent.DeltaPosition.X;
        float GetDeltaY(PointerEvent pointerEvent) => pointerEvent.DeltaPosition.Y;

        protected override string BuildButtonName() =>
            PointerId < 0 ? base.BuildButtonName() : Type.ToString() + PointerId + "." + ShortName;
    }
}
