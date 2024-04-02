namespace Vixen.InputSystem.VirtualButtons;

/// <summary>
///     Describes a virtual button (a key from a keyboard, a mouse button, an axis of a joystick...etc.).
/// </summary>
public partial class VirtualButton {
    /// <summary>
    ///     Keyboard virtual button.
    /// </summary>
    public class Keyboard : VirtualButton {
        /// <summary>
        ///     The 'none' key.
        /// </summary>
        public static readonly VirtualButton None = new Keyboard("none", (int)Key.None);

        /// <summary>
        ///     The 'cancel' key.
        /// </summary>
        public static readonly VirtualButton Cancel = new Keyboard("cancel", (int)Key.Cancel);

        /// <summary>
        ///     The 'tab' key.
        /// </summary>
        public static readonly VirtualButton Tab = new Keyboard("tab", (int)Key.Tab);

        /// <summary>
        ///     The 'linefeed' key.
        /// </summary>
        public static readonly VirtualButton LineFeed = new Keyboard("linefeed", (int)Key.LineFeed);

        /// <summary>
        ///     The 'clear' key.
        /// </summary>
        public static readonly VirtualButton Clear = new Keyboard("clear", (int)Key.Clear);

        /// <summary>
        ///     The 'enter' key.
        /// </summary>
        public static readonly VirtualButton Enter = new Keyboard("enter", (int)Key.Enter);

        /// <summary>
        ///     The 'return' key.
        /// </summary>
        public static readonly VirtualButton Return = new Keyboard("return", (int)Key.Return);

        /// <summary>
        ///     The 'pause' key.
        /// </summary>
        public static readonly VirtualButton Pause = new Keyboard("pause", (int)Key.Pause);

        /// <summary>
        ///     The 'capslock' key.
        /// </summary>
        public static readonly VirtualButton CapsLock = new Keyboard("capslock", (int)Key.CapsLock);

        /// <summary>
        ///     The 'hangulmode' key.
        /// </summary>
        public static readonly VirtualButton HangulMode = new Keyboard("hangulmode", (int)Key.HangulMode);

        /// <summary>
        ///     The 'kanamode' key.
        /// </summary>
        public static readonly VirtualButton KanaMode = new Keyboard("kanamode", (int)Key.KanaMode);

        /// <summary>
        ///     The 'junjamode' key.
        /// </summary>
        public static readonly VirtualButton JunjaMode = new Keyboard("junjamode", (int)Key.JunjaMode);

        /// <summary>
        ///     The 'finalmode' key.
        /// </summary>
        public static readonly VirtualButton FinalMode = new Keyboard("finalmode", (int)Key.FinalMode);

        /// <summary>
        ///     The 'hanjamode' key.
        /// </summary>
        public static readonly VirtualButton HanjaMode = new Keyboard("hanjamode", (int)Key.HanjaMode);

        /// <summary>
        ///     The 'kanjimode' key.
        /// </summary>
        public static readonly VirtualButton KanjiMode = new Keyboard("kanjimode", (int)Key.KanjiMode);

        /// <summary>
        ///     The 'escape' key.
        /// </summary>
        public static readonly VirtualButton Escape = new Keyboard("escape", (int)Key.Escape);

        /// <summary>
        ///     The 'imeconvert' key.
        /// </summary>
        public static readonly VirtualButton ImeConvert = new Keyboard("imeconvert", (int)Key.ImeConvert);

        /// <summary>
        ///     The 'imenonconvert' key.
        /// </summary>
        public static readonly VirtualButton ImeNonConvert = new Keyboard("imenonconvert", (int)Key.ImeNonConvert);

        /// <summary>
        ///     The 'imeaccept' key.
        /// </summary>
        public static readonly VirtualButton ImeAccept = new Keyboard("imeaccept", (int)Key.ImeAccept);

        /// <summary>
        ///     The 'imemodechange' key.
        /// </summary>
        public static readonly VirtualButton ImeModeChange = new Keyboard("imemodechange", (int)Key.ImeModeChange);

        /// <summary>
        ///     The 'space' key.
        /// </summary>
        public static readonly VirtualButton Space = new Keyboard("space", (int)Key.Space);

        /// <summary>
        ///     The 'pageup' key.
        /// </summary>
        public static readonly VirtualButton PageUp = new Keyboard("pageup", (int)Key.PageUp);

        /// <summary>
        ///     The 'prior' key.
        /// </summary>
        public static readonly VirtualButton Prior = new Keyboard("prior", (int)Key.Prior);

        /// <summary>
        ///     The 'next' key.
        /// </summary>
        public static readonly VirtualButton Next = new Keyboard("next", (int)Key.Next);

        /// <summary>
        ///     The 'pagedown' key.
        /// </summary>
        public static readonly VirtualButton PageDown = new Keyboard("pagedown", (int)Key.PageDown);

        /// <summary>
        ///     The 'end' key.
        /// </summary>
        public static readonly VirtualButton End = new Keyboard("end", (int)Key.End);

        /// <summary>
        ///     The 'home' key.
        /// </summary>
        public static readonly VirtualButton Home = new Keyboard("home", (int)Key.Home);

        /// <summary>
        ///     The 'left' key.
        /// </summary>
        public static readonly VirtualButton Left = new Keyboard("left", (int)Key.Left);

        /// <summary>
        ///     The 'up' key.
        /// </summary>
        public static readonly VirtualButton Up = new Keyboard("up", (int)Key.Up);

        /// <summary>
        ///     The 'right' key.
        /// </summary>
        public static readonly VirtualButton Right = new Keyboard("right", (int)Key.Right);

        /// <summary>
        ///     The 'down' key.
        /// </summary>
        public static readonly VirtualButton Down = new Keyboard("down", (int)Key.Down);

        /// <summary>
        ///     The 'select' key.
        /// </summary>
        public static readonly VirtualButton Select = new Keyboard("select", (int)Key.Select);

        /// <summary>
        ///     The 'print' key.
        /// </summary>
        public static readonly VirtualButton Print = new Keyboard("print", (int)Key.Print);

        /// <summary>
        ///     The 'execute' key.
        /// </summary>
        public static readonly VirtualButton Execute = new Keyboard("execute", (int)Key.Execute);

        /// <summary>
        ///     The 'printscreen' key.
        /// </summary>
        public static readonly VirtualButton PrintScreen = new Keyboard("printscreen", (int)Key.PrintScreen);

        /// <summary>
        ///     The 'snapshot' key.
        /// </summary>
        public static readonly VirtualButton Snapshot = new Keyboard("snapshot", (int)Key.Snapshot);

        /// <summary>
        ///     The 'insert' key.
        /// </summary>
        public static readonly VirtualButton Insert = new Keyboard("insert", (int)Key.Insert);

        /// <summary>
        ///     The 'delete' key.
        /// </summary>
        public static readonly VirtualButton Delete = new Keyboard("delete", (int)Key.Delete);

        /// <summary>
        ///     The 'help' key.
        /// </summary>
        public static readonly VirtualButton Help = new Keyboard("help", (int)Key.Help);

        /// <summary>
        ///     The 'd0' key.
        /// </summary>
        public static readonly VirtualButton D0 = new Keyboard("d0", (int)Key.Number0);

        /// <summary>
        ///     The 'd1' key.
        /// </summary>
        public static readonly VirtualButton D1 = new Keyboard("d1", (int)Key.Number1);

        /// <summary>
        ///     The 'd2' key.
        /// </summary>
        public static readonly VirtualButton D2 = new Keyboard("d2", (int)Key.Number2);

        /// <summary>
        ///     The 'd3' key.
        /// </summary>
        public static readonly VirtualButton D3 = new Keyboard("d3", (int)Key.Number3);

        /// <summary>
        ///     The 'd4' key.
        /// </summary>
        public static readonly VirtualButton D4 = new Keyboard("d4", (int)Key.Number4);

        /// <summary>
        ///     The 'd5' key.
        /// </summary>
        public static readonly VirtualButton D5 = new Keyboard("d5", (int)Key.Number5);

        /// <summary>
        ///     The 'd6' key.
        /// </summary>
        public static readonly VirtualButton D6 = new Keyboard("d6", (int)Key.Number6);

        /// <summary>
        ///     The 'd7' key.
        /// </summary>
        public static readonly VirtualButton D7 = new Keyboard("d7", (int)Key.Number7);

        /// <summary>
        ///     The 'd8' key.
        /// </summary>
        public static readonly VirtualButton D8 = new Keyboard("d8", (int)Key.Number8);

        /// <summary>
        ///     The 'd9' key.
        /// </summary>
        public static readonly VirtualButton D9 = new Keyboard("d9", (int)Key.Number9);

        /// <summary>
        ///     The 'a' key.
        /// </summary>
        public static readonly VirtualButton A = new Keyboard("a", (int)Key.A);

        /// <summary>
        ///     The 'b' key.
        /// </summary>
        public static readonly VirtualButton B = new Keyboard("b", (int)Key.B);

        /// <summary>
        ///     The 'c' key.
        /// </summary>
        public static readonly VirtualButton C = new Keyboard("c", (int)Key.C);

        /// <summary>
        ///     The 'd' key.
        /// </summary>
        public static readonly VirtualButton D = new Keyboard("d", (int)Key.D);

        /// <summary>
        ///     The 'e' key.
        /// </summary>
        public static readonly VirtualButton E = new Keyboard("e", (int)Key.E);

        /// <summary>
        ///     The 'f' key.
        /// </summary>
        public static readonly VirtualButton F = new Keyboard("f", (int)Key.F);

        /// <summary>
        ///     The 'g' key.
        /// </summary>
        public static readonly VirtualButton G = new Keyboard("g", (int)Key.G);

        /// <summary>
        ///     The 'h' key.
        /// </summary>
        public static readonly VirtualButton H = new Keyboard("h", (int)Key.H);

        /// <summary>
        ///     The 'i' key.
        /// </summary>
        public static readonly VirtualButton I = new Keyboard("i", (int)Key.I);

        /// <summary>
        ///     The 'j' key.
        /// </summary>
        public static readonly VirtualButton J = new Keyboard("j", (int)Key.J);

        /// <summary>
        ///     The 'k' key.
        /// </summary>
        public static readonly VirtualButton K = new Keyboard("k", (int)Key.K);

        /// <summary>
        ///     The 'l' key.
        /// </summary>
        public static readonly VirtualButton L = new Keyboard("l", (int)Key.L);

        /// <summary>
        ///     The 'm' key.
        /// </summary>
        public static readonly VirtualButton M = new Keyboard("m", (int)Key.M);

        /// <summary>
        ///     The 'n' key.
        /// </summary>
        public static readonly VirtualButton N = new Keyboard("n", (int)Key.N);

        /// <summary>
        ///     The 'o' key.
        /// </summary>
        public static readonly VirtualButton O = new Keyboard("o", (int)Key.O);

        /// <summary>
        ///     The 'p' key.
        /// </summary>
        public static readonly VirtualButton P = new Keyboard("p", (int)Key.P);

        /// <summary>
        ///     The 'q' key.
        /// </summary>
        public static readonly VirtualButton Q = new Keyboard("q", (int)Key.Q);

        /// <summary>
        ///     The 'r' key.
        /// </summary>
        public static readonly VirtualButton R = new Keyboard("r", (int)Key.R);

        /// <summary>
        ///     The 's' key.
        /// </summary>
        public static readonly VirtualButton S = new Keyboard("s", (int)Key.S);

        /// <summary>
        ///     The 't' key.
        /// </summary>
        public static readonly VirtualButton T = new Keyboard("t", (int)Key.T);

        /// <summary>
        ///     The 'u' key.
        /// </summary>
        public static readonly VirtualButton U = new Keyboard("u", (int)Key.U);

        /// <summary>
        ///     The 'v' key.
        /// </summary>
        public static readonly VirtualButton V = new Keyboard("v", (int)Key.V);

        /// <summary>
        ///     The 'w' key.
        /// </summary>
        public static readonly VirtualButton W = new Keyboard("w", (int)Key.W);

        /// <summary>
        ///     The 'x' key.
        /// </summary>
        public static readonly VirtualButton X = new Keyboard("x", (int)Key.X);

        /// <summary>
        ///     The 'y' key.
        /// </summary>
        public static readonly VirtualButton Y = new Keyboard("y", (int)Key.Y);

        /// <summary>
        ///     The 'z' key.
        /// </summary>
        public static readonly VirtualButton Z = new Keyboard("z", (int)Key.Z);

        /// <summary>
        ///     The 'leftwin' key.
        /// </summary>
        public static readonly VirtualButton LeftWin = new Keyboard("leftwin", (int)Key.LeftSuper);

        /// <summary>
        ///     The 'rightwin' key.
        /// </summary>
        public static readonly VirtualButton RightWin = new Keyboard("rightwin", (int)Key.RightSuper);

        /// <summary>
        ///     The 'apps' key.
        /// </summary>
        public static readonly VirtualButton Apps = new Keyboard("apps", (int)Key.Menu);

        /// <summary>
        ///     The 'sleep' key.
        /// </summary>
        public static readonly VirtualButton Sleep = new Keyboard("sleep", (int)Key.Sleep);

        /// <summary>
        ///     The 'numpad0' key.
        /// </summary>
        public static readonly VirtualButton NumPad0 = new Keyboard("numpad0", (int)Key.NumPad0);

        /// <summary>
        ///     The 'numpad1' key.
        /// </summary>
        public static readonly VirtualButton NumPad1 = new Keyboard("numpad1", (int)Key.NumPad1);

        /// <summary>
        ///     The 'numpad2' key.
        /// </summary>
        public static readonly VirtualButton NumPad2 = new Keyboard("numpad2", (int)Key.NumPad2);

        /// <summary>
        ///     The 'numpad3' key.
        /// </summary>
        public static readonly VirtualButton NumPad3 = new Keyboard("numpad3", (int)Key.NumPad3);

        /// <summary>
        ///     The 'numpad4' key.
        /// </summary>
        public static readonly VirtualButton NumPad4 = new Keyboard("numpad4", (int)Key.NumPad4);

        /// <summary>
        ///     The 'numpad5' key.
        /// </summary>
        public static readonly VirtualButton NumPad5 = new Keyboard("numpad5", (int)Key.NumPad5);

        /// <summary>
        ///     The 'numpad6' key.
        /// </summary>
        public static readonly VirtualButton NumPad6 = new Keyboard("numpad6", (int)Key.NumPad6);

        /// <summary>
        ///     The 'numpad7' key.
        /// </summary>
        public static readonly VirtualButton NumPad7 = new Keyboard("numpad7", (int)Key.NumPad7);

        /// <summary>
        ///     The 'numpad8' key.
        /// </summary>
        public static readonly VirtualButton NumPad8 = new Keyboard("numpad8", (int)Key.NumPad8);

        /// <summary>
        ///     The 'numpad9' key.
        /// </summary>
        public static readonly VirtualButton NumPad9 = new Keyboard("numpad9", (int)Key.NumPad9);

        /// <summary>
        ///     The 'multiply' key.
        /// </summary>
        public static readonly VirtualButton Multiply = new Keyboard("multiply", (int)Key.Multiply);

        /// <summary>
        ///     The 'add' key.
        /// </summary>
        public static readonly VirtualButton Add = new Keyboard("add", (int)Key.Add);

        /// <summary>
        ///     The 'separator' key.
        /// </summary>
        public static readonly VirtualButton Separator = new Keyboard("separator", (int)Key.Separator);

        /// <summary>
        ///     The 'subtract' key.
        /// </summary>
        public static readonly VirtualButton Subtract = new Keyboard("subtract", (int)Key.Subtract);

        /// <summary>
        ///     The 'decimal' key.
        /// </summary>
        public static readonly VirtualButton Decimal = new Keyboard("decimal", (int)Key.Decimal);

        /// <summary>
        ///     The 'divide' key.
        /// </summary>
        public static readonly VirtualButton Divide = new Keyboard("divide", (int)Key.Divide);

        /// <summary>
        ///     The 'f1' key.
        /// </summary>
        public static readonly VirtualButton F1 = new Keyboard("f1", (int)Key.F1);

        /// <summary>
        ///     The 'f2' key.
        /// </summary>
        public static readonly VirtualButton F2 = new Keyboard("f2", (int)Key.F2);

        /// <summary>
        ///     The 'f3' key.
        /// </summary>
        public static readonly VirtualButton F3 = new Keyboard("f3", (int)Key.F3);

        /// <summary>
        ///     The 'f4' key.
        /// </summary>
        public static readonly VirtualButton F4 = new Keyboard("f4", (int)Key.F4);

        /// <summary>
        ///     The 'f5' key.
        /// </summary>
        public static readonly VirtualButton F5 = new Keyboard("f5", (int)Key.F5);

        /// <summary>
        ///     The 'f6' key.
        /// </summary>
        public static readonly VirtualButton F6 = new Keyboard("f6", (int)Key.F6);

        /// <summary>
        ///     The 'f7' key.
        /// </summary>
        public static readonly VirtualButton F7 = new Keyboard("f7", (int)Key.F7);

        /// <summary>
        ///     The 'f8' key.
        /// </summary>
        public static readonly VirtualButton F8 = new Keyboard("f8", (int)Key.F8);

        /// <summary>
        ///     The 'f9' key.
        /// </summary>
        public static readonly VirtualButton F9 = new Keyboard("f9", (int)Key.F9);

        /// <summary>
        ///     The 'f10' key.
        /// </summary>
        public static readonly VirtualButton F10 = new Keyboard("f10", (int)Key.F10);

        /// <summary>
        ///     The 'f11' key.
        /// </summary>
        public static readonly VirtualButton F11 = new Keyboard("f11", (int)Key.F11);

        /// <summary>
        ///     The 'f12' key.
        /// </summary>
        public static readonly VirtualButton F12 = new Keyboard("f12", (int)Key.F12);

        /// <summary>
        ///     The 'f13' key.
        /// </summary>
        public static readonly VirtualButton F13 = new Keyboard("f13", (int)Key.F13);

        /// <summary>
        ///     The 'f14' key.
        /// </summary>
        public static readonly VirtualButton F14 = new Keyboard("f14", (int)Key.F14);

        /// <summary>
        ///     The 'f15' key.
        /// </summary>
        public static readonly VirtualButton F15 = new Keyboard("f15", (int)Key.F15);

        /// <summary>
        ///     The 'f16' key.
        /// </summary>
        public static readonly VirtualButton F16 = new Keyboard("f16", (int)Key.F16);

        /// <summary>
        ///     The 'f17' key.
        /// </summary>
        public static readonly VirtualButton F17 = new Keyboard("f17", (int)Key.F17);

        /// <summary>
        ///     The 'f18' key.
        /// </summary>
        public static readonly VirtualButton F18 = new Keyboard("f18", (int)Key.F18);

        /// <summary>
        ///     The 'f19' key.
        /// </summary>
        public static readonly VirtualButton F19 = new Keyboard("f19", (int)Key.F19);

        /// <summary>
        ///     The 'f20' key.
        /// </summary>
        public static readonly VirtualButton F20 = new Keyboard("f20", (int)Key.F20);

        /// <summary>
        ///     The 'f21' key.
        /// </summary>
        public static readonly VirtualButton F21 = new Keyboard("f21", (int)Key.F21);

        /// <summary>
        ///     The 'f22' key.
        /// </summary>
        public static readonly VirtualButton F22 = new Keyboard("f22", (int)Key.F22);

        /// <summary>
        ///     The 'f23' key.
        /// </summary>
        public static readonly VirtualButton F23 = new Keyboard("f23", (int)Key.F23);

        /// <summary>
        ///     The 'f24' key.
        /// </summary>
        public static readonly VirtualButton F24 = new Keyboard("f24", (int)Key.F24);

        /// <summary>
        ///     The 'numlock' key.
        /// </summary>
        public static readonly VirtualButton NumLock = new Keyboard("numlock", (int)Key.NumLock);

        /// <summary>
        ///     The 'scroll' key.
        /// </summary>
        public static readonly VirtualButton Scroll = new Keyboard("scroll", (int)Key.Scroll);

        /// <summary>
        ///     The 'leftshift' key.
        /// </summary>
        public static readonly VirtualButton LeftShift = new Keyboard("leftshift", (int)Key.LeftShift);

        /// <summary>
        ///     The 'rightshift' key.
        /// </summary>
        public static readonly VirtualButton RightShift = new Keyboard("rightshift", (int)Key.RightShift);

        /// <summary>
        ///     The 'leftctrl' key.
        /// </summary>
        public static readonly VirtualButton LeftCtrl = new Keyboard("leftctrl", (int)Key.LeftCtrl);

        /// <summary>
        ///     The 'rightctrl' key.
        /// </summary>
        public static readonly VirtualButton RightCtrl = new Keyboard("rightctrl", (int)Key.RightCtrl);

        /// <summary>
        ///     The 'leftalt' key.
        /// </summary>
        public static readonly VirtualButton LeftAlt = new Keyboard("leftalt", (int)Key.LeftAlt);

        /// <summary>
        ///     The 'rightalt' key.
        /// </summary>
        public static readonly VirtualButton RightAlt = new Keyboard("rightalt", (int)Key.RightAlt);

        /// <summary>
        ///     The 'browserback' key.
        /// </summary>
        public static readonly VirtualButton BrowserBack = new Keyboard("browserback", (int)Key.BrowserBack);

        /// <summary>
        ///     The 'browserforward' key.
        /// </summary>
        public static readonly VirtualButton BrowserForward = new Keyboard("browserforward", (int)Key.BrowserForward);

        /// <summary>
        ///     The 'browserrefresh' key.
        /// </summary>
        public static readonly VirtualButton BrowserRefresh = new Keyboard("browserrefresh", (int)Key.BrowserRefresh);

        /// <summary>
        ///     The 'browserstop' key.
        /// </summary>
        public static readonly VirtualButton BrowserStop = new Keyboard("browserstop", (int)Key.BrowserStop);

        /// <summary>
        ///     The 'browsersearch' key.
        /// </summary>
        public static readonly VirtualButton BrowserSearch = new Keyboard("browsersearch", (int)Key.BrowserSearch);

        /// <summary>
        ///     The 'browserfavorites' key.
        /// </summary>
        public static readonly VirtualButton BrowserFavorites = new Keyboard(
            "browserfavorites",
            (int)Key.BrowserFavorites
        );

        /// <summary>
        ///     The 'browserhome' key.
        /// </summary>
        public static readonly VirtualButton BrowserHome = new Keyboard("browserhome", (int)Key.BrowserHome);

        /// <summary>
        ///     The 'volumemute' key.
        /// </summary>
        public static readonly VirtualButton VolumeMute = new Keyboard("volumemute", (int)Key.VolumeMute);

        /// <summary>
        ///     The 'volumedown' key.
        /// </summary>
        public static readonly VirtualButton VolumeDown = new Keyboard("volumedown", (int)Key.VolumeDown);

        /// <summary>
        ///     The 'volumeup' key.
        /// </summary>
        public static readonly VirtualButton VolumeUp = new Keyboard("volumeup", (int)Key.VolumeUp);

        /// <summary>
        ///     The 'medianexttrack' key.
        /// </summary>
        public static readonly VirtualButton MediaNextTrack = new Keyboard("medianexttrack", (int)Key.MediaNextTrack);

        /// <summary>
        ///     The 'mediaprevioustrack' key.
        /// </summary>
        public static readonly VirtualButton MediaPreviousTrack = new Keyboard(
            "mediaprevioustrack",
            (int)Key.MediaPreviousTrack
        );

        /// <summary>
        ///     The 'mediastop' key.
        /// </summary>
        public static readonly VirtualButton MediaStop = new Keyboard("mediastop", (int)Key.MediaStop);

        /// <summary>
        ///     The 'mediaplaypause' key.
        /// </summary>
        public static readonly VirtualButton MediaPlayPause = new Keyboard("mediaplaypause", (int)Key.MediaPlayPause);

        /// <summary>
        ///     The 'launchmail' key.
        /// </summary>
        public static readonly VirtualButton LaunchMail = new Keyboard("launchmail", (int)Key.LaunchMail);

        /// <summary>
        ///     The 'selectmedia' key.
        /// </summary>
        public static readonly VirtualButton SelectMedia = new Keyboard("selectmedia", (int)Key.SelectMedia);

        /// <summary>
        ///     The 'launchapplication1' key.
        /// </summary>
        public static readonly VirtualButton LaunchApplication1 = new Keyboard(
            "launchapplication1",
            (int)Key.LaunchApplication1
        );

        /// <summary>
        ///     The 'launchapplication2' key.
        /// </summary>
        public static readonly VirtualButton LaunchApplication2 = new Keyboard(
            "launchapplication2",
            (int)Key.LaunchApplication2
        );

        /// <summary>
        ///     The 'oemsemicolon' key.
        /// </summary>
        public static readonly VirtualButton OemSemicolon = new Keyboard("oemsemicolon", (int)Key.Semicolon);

        /// <summary>
        ///     The 'oemplus' key.
        /// </summary>
        public static readonly VirtualButton OemPlus = new Keyboard("oemplus", (int)Key.Equal);

        /// <summary>
        ///     The 'oemcomma' key.
        /// </summary>
        public static readonly VirtualButton OemComma = new Keyboard("oemcomma", (int)Key.Comma);

        /// <summary>
        ///     The 'oemminus' key.
        /// </summary>
        public static readonly VirtualButton OemMinus = new Keyboard("oemminus", (int)Key.Minus);

        /// <summary>
        ///     The 'oemperiod' key.
        /// </summary>
        public static readonly VirtualButton OemPeriod = new Keyboard("oemperiod", (int)Key.Period);

        /// <summary>
        ///     The 'oem2' key.
        /// </summary>
        public static readonly VirtualButton Oem2 = new Keyboard("oem2", (int)Key.Oem2);

        /// <summary>
        ///     The 'oemquestion' key.
        /// </summary>
        public static readonly VirtualButton OemQuestion = new Keyboard("oemquestion", (int)Key.OemQuestion);

        /// <summary>
        ///     The 'oem3' key.
        /// </summary>
        public static readonly VirtualButton Oem3 = new Keyboard("oem3", (int)Key.Oem3);

        /// <summary>
        ///     The 'oemtilde' key.
        /// </summary>
        public static readonly VirtualButton OemTilde = new Keyboard("oemtilde", (int)Key.OemTilde);

        /// <summary>
        ///     The 'oem4' key.
        /// </summary>
        public static readonly VirtualButton Oem4 = new Keyboard("oem4", (int)Key.Oem4);

        /// <summary>
        ///     The 'oemopenbrackets' key.
        /// </summary>
        public static readonly VirtualButton OemOpenBrackets = new Keyboard(
            "oemopenbrackets",
            (int)Key.OpenBracket
        );

        /// <summary>
        ///     The 'oem5' key.
        /// </summary>
        public static readonly VirtualButton Oem5 = new Keyboard("oem5", (int)Key.Oem5);

        /// <summary>
        ///     The 'oempipe' key.
        /// </summary>
        public static readonly VirtualButton OemPipe = new Keyboard("oempipe", (int)Key.OemPipe);

        /// <summary>
        ///     The 'oem6' key.
        /// </summary>
        public static readonly VirtualButton Oem6 = new Keyboard("oem6", (int)Key.Oem6);

        /// <summary>
        ///     The 'oemclosebrackets' key.
        /// </summary>
        public static readonly VirtualButton OemCloseBrackets = new Keyboard(
            "oemclosebrackets",
            (int)Key.CloseBracket
        );

        /// <summary>
        ///     The 'oem7' key.
        /// </summary>
        public static readonly VirtualButton Oem7 = new Keyboard("oem7", (int)Key.Oem7);

        /// <summary>
        ///     The 'oemquotes' key.
        /// </summary>
        public static readonly VirtualButton OemQuotes = new Keyboard("oemquotes", (int)Key.OemQuotes);

        /// <summary>
        ///     The 'oem8' key.
        /// </summary>
        public static readonly VirtualButton Oem8 = new Keyboard("oem8", (int)Key.Oem8);

        /// <summary>
        ///     The 'oembackslash' key.
        /// </summary>
        public static readonly VirtualButton OemBackslash = new Keyboard("oembackslash", (int)Key.Backslash);

        /// <summary>
        ///     The 'attn' key.
        /// </summary>
        public static readonly VirtualButton Attn = new Keyboard("attn", (int)Key.Attn);

        /// <summary>
        ///     The 'crsel' key.
        /// </summary>
        public static readonly VirtualButton CrSel = new Keyboard("crsel", (int)Key.CrSel);

        /// <summary>
        ///     The 'exsel' key.
        /// </summary>
        public static readonly VirtualButton ExSel = new Keyboard("exsel", (int)Key.ExSel);

        /// <summary>
        ///     The 'eraseeof' key.
        /// </summary>
        public static readonly VirtualButton EraseEof = new Keyboard("eraseeof", (int)Key.EraseEof);

        /// <summary>
        ///     The 'play' key.
        /// </summary>
        public static readonly VirtualButton Play = new Keyboard("play", (int)Key.Play);

        /// <summary>
        ///     The 'zoom' key.
        /// </summary>
        public static readonly VirtualButton Zoom = new Keyboard("zoom", (int)Key.Zoom);

        /// <summary>
        ///     The 'noname' key.
        /// </summary>
        public static readonly VirtualButton NoName = new Keyboard("noname", (int)Key.NoName);

        /// <summary>
        ///     The 'pa1' key.
        /// </summary>
        public static readonly VirtualButton Pa1 = new Keyboard("pa1", (int)Key.Pa1);

        /// <summary>
        ///     The 'oemclear' key.
        /// </summary>
        public static readonly VirtualButton OemClear = new Keyboard("oemclear", (int)Key.OemClear);

        protected Keyboard(string name, int id, bool isPositiveAndNegative = false)
            : base(name, VirtualButtonType.Keyboard, id, isPositiveAndNegative) { }

        public override float GetValue(InputManager manager) => IsDown(manager) ? 1.0f : 0.0f;
        public override bool IsDown(InputManager manager) => manager.IsKeyDown((Key)Index);
        public override bool IsPressed(InputManager manager) => manager.IsKeyPressed((Key)Index);
        public override bool IsReleased(InputManager manager) => manager.IsKeyReleased((Key)Index);
    }
}
