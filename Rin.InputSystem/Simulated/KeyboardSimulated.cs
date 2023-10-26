namespace Rin.InputSystem.Simulated;

public class KeyboardSimulated : KeyboardDeviceBase {
    public override string Name => "Simulated Keyboard";
    public override Guid Id { get; }
    public override IInputSource Source { get; }

    public KeyboardSimulated(InputSourceSimulated source) {
        Priority = -1000;
        Source = source;

        Id = Guid.NewGuid();
    }

    public void SimulateDown(Key key) {
        HandleKeyDown(key);
    }

    public void SimulateUp(Key key) {
        HandleKeyUp(key);
    }
}
