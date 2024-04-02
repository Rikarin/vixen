using Vixen.InputSystem;
using ISilkGamepad = Silk.NET.Input.IGamepad;

namespace Vixen.Platform.Silk.Input;

public class SilkGameController : GameControllerDeviceBase, IDisposable {
    readonly List<GameControllerButtonInfo> buttonInfos = new();
    readonly List<GameControllerAxisInfo> axisInfos = new();
    readonly List<GameControllerDirectionInfo> povControllerInfos = new();
    readonly ISilkGamepad silkGamepad;
    public override string Name { get; }
    public override Guid Id { get; }
    public override IInputSource Source { get; }
    public override IReadOnlyList<GameControllerButtonInfo> ButtonInfos => buttonInfos;
    public override IReadOnlyList<GameControllerAxisInfo> AxisInfos => axisInfos;
    public override IReadOnlyList<GameControllerDirectionInfo> DirectionInfos => povControllerInfos;

    public SilkGameController(IInputSource inputSource, ISilkGamepad silkGamepad) {
        Source = inputSource;
        this.silkGamepad = silkGamepad;

        Id = Guid.NewGuid();
        Name = silkGamepad.Name;

        foreach (var button in silkGamepad.Buttons) {
            buttonInfos.Add(new() { Name = $"Button {button.Index}"});
        }
        
        foreach (var axis in silkGamepad.Thumbsticks) {
            axisInfos.Add(new() { Name = $"Axis {axis.Index}"});
        }
        
        foreach (var trigger in silkGamepad.Triggers) {
            povControllerInfos.Add(new() { Name = $"Trigger {trigger.Index}"});
        }
        
        InitializeButtonStates();
    }

    public void Dispose() {
        // TODO release managed resources here
    }
}
