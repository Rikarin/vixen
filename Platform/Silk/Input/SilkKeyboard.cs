using Rin.InputSystem;
using Silk.NET.Input;
using Key = Rin.InputSystem.Key;
using SilkKey = Silk.NET.Input.Key;

namespace Rin.Platform.Silk.Input;

public class SilkKeyboard : KeyboardDeviceBase, ITextInputDevice, IDisposable {
    readonly IKeyboard silkKeyboard;

    public override string Name { get; }
    public override Guid Id { get; }
    public override IInputSource Source { get; }

    public SilkKeyboard(IInputSource inputSource, IKeyboard silkKeyboard) {
        this.silkKeyboard = silkKeyboard;
        Source = inputSource;

        // TODO
        Id = new("4A097B06-F340-4E10-B293-177BADE4B322");
        Name = this.silkKeyboard.Name;

        silkKeyboard.KeyDown += OnKeyDown;
        silkKeyboard.KeyUp += OnKeyUp;
    }

    public void EnabledTextInput() {
        throw new NotImplementedException();
    }

    public void DisableTextInput() {
        throw new NotImplementedException();
    }

    public void Dispose() {
        silkKeyboard.KeyDown -= OnKeyDown;
        silkKeyboard.KeyUp -= OnKeyUp;
    }

    void OnKeyUp(IKeyboard keyboard, SilkKey key, int arg3) => HandleKeyUp(MapKey(key));
    void OnKeyDown(IKeyboard keyboard, SilkKey key, int arg3) => HandleKeyDown(MapKey(key));

    Key MapKey(SilkKey silkKey) =>
        silkKey switch {
            SilkKey.Unknown => Key.None,
            SilkKey.A => Key.A,
            SilkKey.B => Key.B,
            SilkKey.C => Key.C,
            SilkKey.D => Key.D,
            SilkKey.E => Key.E,
            SilkKey.F => Key.F,
            SilkKey.G => Key.G,
            SilkKey.H => Key.H,
            SilkKey.I => Key.I,
            SilkKey.J => Key.J,
            SilkKey.K => Key.K,
            SilkKey.L => Key.L,
            SilkKey.M => Key.M,
            SilkKey.N => Key.N,
            SilkKey.O => Key.O,
            SilkKey.P => Key.P,
            SilkKey.Q => Key.Q,
            SilkKey.R => Key.R,
            SilkKey.S => Key.S,
            SilkKey.T => Key.T,
            SilkKey.U => Key.U,
            SilkKey.V => Key.V,
            SilkKey.W => Key.W,
            SilkKey.X => Key.X,
            SilkKey.Y => Key.Y,
            SilkKey.Z => Key.Z,
            SilkKey.Space => Key.Space,
            // SilkKey.Apostrophe => Key.,
            SilkKey.Comma => Key.Comma,
            SilkKey.Minus => Key.Minus,
            SilkKey.Period => Key.Period,
            // SilkKey.Slash => Key.,
            SilkKey.Number0 => Key.Number0,
            SilkKey.Number1 => Key.Number1,
            SilkKey.Number2 => Key.Number2,
            SilkKey.Number3 => Key.Number3,
            SilkKey.Number4 => Key.Number4,
            SilkKey.Number5 => Key.Number5,
            SilkKey.Number6 => Key.Number6,
            SilkKey.Number7 => Key.Number7,
            SilkKey.Number8 => Key.Number8,
            SilkKey.Number9 => Key.Number9,
            SilkKey.Semicolon => Key.Semicolon,
            SilkKey.Equal => Key.Equal,
            SilkKey.LeftBracket => Key.OpenBracket,
            SilkKey.BackSlash => Key.Backslash,
            SilkKey.RightBracket => Key.CloseBracket,
            // SilkKey.GraveAccent => expr,
            // SilkKey.World1 => expr,
            // SilkKey.World2 => expr,
            SilkKey.Escape => Key.Escape,
            SilkKey.Enter => Key.Enter,
            SilkKey.Tab => Key.Tab,
            SilkKey.Backspace => Key.BackSpace,
            SilkKey.Insert => Key.Insert,
            SilkKey.Delete => Key.Delete,
            SilkKey.Right => Key.Right,
            SilkKey.Left => Key.Left,
            SilkKey.Down => Key.Down,
            SilkKey.Up => Key.Up,
            SilkKey.PageUp => Key.PageUp,
            SilkKey.PageDown => Key.PageDown,
            SilkKey.Home => Key.Home,
            SilkKey.End => Key.End,
            SilkKey.CapsLock => Key.CapsLock,
            SilkKey.ScrollLock => Key.Scroll,
            SilkKey.NumLock => Key.NumLock,
            SilkKey.PrintScreen => Key.PrintScreen,
            SilkKey.Pause => Key.Pause,
            SilkKey.F1 => Key.F1,
            SilkKey.F2 => Key.F2,
            SilkKey.F3 => Key.F3,
            SilkKey.F4 => Key.F4,
            SilkKey.F5 => Key.F5,
            SilkKey.F6 => Key.F6,
            SilkKey.F7 => Key.F7,
            SilkKey.F8 => Key.F8,
            SilkKey.F9 => Key.F9,
            SilkKey.F10 => Key.F10,
            SilkKey.F11 => Key.F11,
            SilkKey.F12 => Key.F12,
            SilkKey.F13 => Key.F13,
            SilkKey.F14 => Key.F14,
            SilkKey.F15 => Key.F15,
            SilkKey.F16 => Key.F16,
            SilkKey.F17 => Key.F17,
            SilkKey.F18 => Key.F18,
            SilkKey.F19 => Key.F19,
            SilkKey.F20 => Key.F20,
            SilkKey.F21 => Key.F21,
            SilkKey.F22 => Key.F22,
            SilkKey.F23 => Key.F23,
            SilkKey.F24 => Key.F24,
            // SilkKey.F25 => Key.F25,
            SilkKey.Keypad0 => Key.NumPad0,
            SilkKey.Keypad1 => Key.NumPad1,
            SilkKey.Keypad2 => Key.NumPad2,
            SilkKey.Keypad3 => Key.NumPad3,
            SilkKey.Keypad4 => Key.NumPad4,
            SilkKey.Keypad5 => Key.NumPad5,
            SilkKey.Keypad6 => Key.NumPad6,
            SilkKey.Keypad7 => Key.NumPad7,
            SilkKey.Keypad8 => Key.NumPad8,
            SilkKey.Keypad9 => Key.NumPad9,
            SilkKey.KeypadDecimal => Key.Decimal,
            SilkKey.KeypadDivide => Key.Divide,
            SilkKey.KeypadMultiply => Key.Multiply,
            SilkKey.KeypadSubtract => Key.Subtract,
            SilkKey.KeypadAdd => Key.Add,
            SilkKey.KeypadEnter => Key.NumPadEnter,
            // SilkKey.KeypadEqual => Key.,
            SilkKey.ShiftLeft => Key.LeftShift,
            SilkKey.ControlLeft => Key.LeftCtrl,
            SilkKey.AltLeft => Key.LeftAlt,
            SilkKey.SuperLeft => Key.LeftSuper,
            SilkKey.ShiftRight => Key.RightShift,
            SilkKey.ControlRight => Key.RightCtrl,
            SilkKey.AltRight => Key.RightAlt,
            SilkKey.SuperRight => Key.RightSuper,
            SilkKey.Menu => Key.Menu,

            _ => Key.None
        };
}
