using Rin.InputSystem;
using Silk.NET.Input;
using ISilkWindow = Silk.NET.Windowing.IWindow;


namespace Rin.Platform.Silk.Input; 

public class SilkInputSource : InputSourceBase {
    readonly ISilkWindow silkWindow;
    internal IInputContext SilkInputContext { get; private set; }
    
    SilkKeyboard keyboard;
    SilkMouse mouse;

    public SilkInputSource(ISilkWindow silkWindow) {
        this.silkWindow = silkWindow;
    }
    
    public override void Initialize(InputManager inputManager) {
        SilkInputContext = silkWindow.CreateInput();
        
        keyboard = new(this, SilkInputContext.Keyboards.First());
        mouse = new(this, SilkInputContext.Mice.First(), silkWindow);

        foreach (var gamepad in SilkInputContext.Gamepads) {
            var controller = new SilkGameController(this, gamepad);
            // We don't need to find layout as this is done by Silk.NET for us
            
            //     Log.Information("gamepad: {Variable}", gamepad.Name);
            //     gamepad.ButtonDown += (gamepad1, button) => Log.Information("Debug: {Variable}", button.Name);
            //     gamepad.ThumbstickMoved += (gamepad1, thumbstick) => Log.Information("Debug: {@Variable}", thumbstick);
            //     gamepad.TriggerMoved += (gamepad1, trigger) => Log.Information("Debug: {@Variable}", trigger);
        }
        
        // foreach (var gamepad in SilkInputContext.Joysticks) {
        //     Log.Information("joystick: {Variable}", gamepad.Name);
        // }
        
        RegisterDevice(keyboard);
        RegisterDevice(mouse);
    }
}
