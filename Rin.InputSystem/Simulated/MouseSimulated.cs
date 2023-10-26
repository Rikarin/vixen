using System.Numerics;

namespace Rin.InputSystem.Simulated;

public class MouseSimulated : MouseDeviceBase {
    bool positionLocked;
    Vector2 capturedPosition;

    public override string Name => "Simulated Mouse";
    public override Guid Id { get; }
    public override bool IsPositionLocked => positionLocked;
    public override IInputSource Source { get; }

    public MouseDeviceState MouseState => base.MouseState;
    public PointerDeviceState PointerState => base.PointerState;

    public MouseSimulated(InputSourceSimulated source) {
        Priority = -1000;
        SetSurfaceSize(Vector2.One);
        Source = source;

        Id = Guid.NewGuid();
    }

    public override void Update(List<InputEvent> inputEvents) {
        base.Update(inputEvents);

        if (positionLocked) {
            MouseState.Position = capturedPosition;
            PointerState.GetPointerData(0).Position = capturedPosition;
        }
    }

    public void SimulateMouseDown(MouseButton button) {
        MouseState.HandleButtonDown(button);
    }

    public void SimulateMouseUp(MouseButton button) {
        MouseState.HandleButtonUp(button);
    }

    public void SimulateMouseWheel(float wheelDelta) {
        MouseState.HandleMouseWheel(wheelDelta);
    }

    public override void SetPosition(Vector2 position) {
        if (IsPositionLocked) {
            MouseState.HandleMouseDelta(position * SurfaceSize - capturedPosition);
        } else {
            MouseState.HandleMove(position * SurfaceSize);
        }
    }

    public void SimulatePointer(PointerEventType pointerEventType, Vector2 position, int id = 0) {
        PointerState.PointerInputEvents.Add(new() { Id = id, Position = position, Type = pointerEventType });
    }

    public override void LockPosition(bool forceCenter = false) {
        positionLocked = true;
        capturedPosition = forceCenter ? new(0.5f) : Position;
    }

    public override void UnlockPosition() {
        positionLocked = false;
    }
}
