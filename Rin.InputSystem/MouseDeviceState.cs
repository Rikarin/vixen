using System.Numerics;

namespace Rin.InputSystem;

/// <summary>
///     An extension to <see cref="PointerDeviceState" /> that handle mouse input and translates it to pointer input
/// </summary>
public class MouseDeviceState {
    Vector2 nextDelta = Vector2.Zero;

    readonly HashSet<MouseButton> pressedButtons = new();
    readonly HashSet<MouseButton> releasedButtons = new();
    readonly HashSet<MouseButton> downButtons = new();

    public IReadOnlySet<MouseButton> PressedButtons => pressedButtons;
    public IReadOnlySet<MouseButton> ReleasedButtons => releasedButtons;
    public IReadOnlySet<MouseButton> DownButtons => downButtons;

    public Vector2 Position { get; set; }
    public Vector2 Delta { get; set; }
    protected List<InputEvent> Events { get; } = new();

    protected IMouseDevice MouseDevice { get; }
    protected PointerDeviceState PointerState { get; }

    public MouseDeviceState(PointerDeviceState pointerState, IMouseDevice mouseDevice) {
        PointerState = pointerState;
        MouseDevice = mouseDevice;
    }

    /// <summary>
    ///     Generate input events
    /// </summary>
    public void Update(List<InputEvent> inputEvents) {
        Reset();

        // Collect events from queue
        foreach (var evt in Events) {
            inputEvents.Add(evt);

            if (evt is MouseButtonEvent mouseButtonEvent) {
                if (mouseButtonEvent.IsDown) {
                    pressedButtons.Add(mouseButtonEvent.Button);
                } else {
                    releasedButtons.Add(mouseButtonEvent.Button);
                }
            }

            // Pass mouse-side generate pointer events through the pointer state
            // These should only be delta movement events so don't update it from this functions
            if (evt is PointerEvent pointerEvent) {
                PointerState.UpdatePointerState(pointerEvent, false);
            }
        }

        Events.Clear();

        // Reset mouse delta
        Delta = nextDelta;
        nextDelta = Vector2.Zero;
    }

    /// <summary>
    ///     Special move that generates pointer events with just delta
    /// </summary>
    /// <param name="delta">The movement delta</param>
    public void HandleMouseDelta(Vector2 delta) {
        if (delta == Vector2.Zero) {
            return;
        }

        // Normalize delta
        delta *= PointerState.InverseSurfaceSize;

        nextDelta += delta;

        var pointerEvent = InputEventPool<PointerEvent>.GetOrCreate(MouseDevice);
        pointerEvent.Position = Position;
        pointerEvent.DeltaPosition = delta;
        pointerEvent.PointerId = 0;
        pointerEvent.EventType = PointerEventType.Moved;

        Events.Add(pointerEvent);
    }

    public void HandleButtonDown(MouseButton button) {
        // Prevent duplicate events
        if (downButtons.Contains(button)) {
            return;
        }

        downButtons.Add(button);

        var buttonEvent = InputEventPool<MouseButtonEvent>.GetOrCreate(MouseDevice);
        buttonEvent.Button = button;
        buttonEvent.IsDown = true;
        Events.Add(buttonEvent);

        // Simulate tap on primary mouse button
        if (button == MouseButton.Left) {
            HandlePointerDown();
        }
    }

    public void HandleButtonUp(MouseButton button) {
        // Prevent duplicate events
        if (!downButtons.Contains(button)) {
            return;
        }

        downButtons.Remove(button);

        var buttonEvent = InputEventPool<MouseButtonEvent>.GetOrCreate(MouseDevice);
        buttonEvent.Button = button;
        buttonEvent.IsDown = false;
        Events.Add(buttonEvent);

        // Simulate tap on primary mouse button
        if (button == MouseButton.Left) {
            HandlePointerUp();
        }
    }

    public void HandleMouseWheel(float wheelDelta) {
        var wheelEvent = InputEventPool<MouseWheelEvent>.GetOrCreate(MouseDevice);
        wheelEvent.WheelDelta = wheelDelta;
        Events.Add(wheelEvent);
    }

    /// <summary>
    ///     Handles a single pointer down
    /// </summary>
    public void HandlePointerDown() {
        PointerState.PointerInputEvents.Add(new() { Type = PointerEventType.Pressed, Position = Position, Id = 0 });
    }

    /// <summary>
    ///     Handles a single pointer up
    /// </summary>
    public void HandlePointerUp() {
        PointerState.PointerInputEvents.Add(new() { Type = PointerEventType.Released, Position = Position, Id = 0 });
    }

    /// <summary>
    ///     Handles a single pointer move
    /// </summary>
    /// <param name="newPosition">New position of the pointer</param>
    public void HandleMove(Vector2 newPosition) {
        // Normalize position
        newPosition *= PointerState.InverseSurfaceSize;

        if (newPosition != Position) {
            nextDelta += newPosition - Position;
            Position = newPosition;

            // Generate Event
            PointerState.PointerInputEvents.Add(
                new() { Type = PointerEventType.Moved, Position = newPosition, Id = 0 }
            );
        }
    }

    void Reset() {
        pressedButtons.Clear();
        releasedButtons.Clear();
    }
}
