using System.Numerics;

namespace Vixen.InputSystem.Simulated;

/// <summary>
///     Simulation of PointerEvents
/// </summary>
public class PointerSimulated : PointerDeviceBase {
    public override string Name => "Simulated Pointer";
    public override Guid Id { get; }
    public override IInputSource Source { get; }

    public PointerDeviceState PointerState => base.PointerState;

    public PointerSimulated(InputSourceSimulated source) {
        Priority = -1000;
        SetSurfaceSize(Vector2.One);
        Source = source;

        Id = Guid.NewGuid();
    }

    public override void Update(List<InputEvent> inputEvents) {
        base.Update(inputEvents);
    }

    public void SimulatePointer(PointerEventType pointerEventType, Vector2 position, int id = 0) {
        PointerState.PointerInputEvents.Add(new() { Id = id, Position = position, Type = pointerEventType });
    }

    //shortcuts for convenience
    public void MovePointer(Vector2 position, int id = 0) {
        SimulatePointer(PointerEventType.Moved, position, id);
    }

    public void PressPointer(Vector2 position, int id = 0) {
        SimulatePointer(PointerEventType.Pressed, position, id);
    }

    public void ReleasePointer(Vector2 position, int id = 0) {
        SimulatePointer(PointerEventType.Released, position, id);
    }

    public void CancelPointer(Vector2 position, int id = 0) {
        SimulatePointer(PointerEventType.Canceled, position, id);
    }
}
