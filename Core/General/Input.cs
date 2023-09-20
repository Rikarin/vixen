using Rin.Core.Abstractions;
using System.Numerics;

namespace Rin.Core.General;

// TODO: finish this
public static class Input {
    public static Vector2 MousePosition => Application.Window.handle.MousePosition;

    public static bool GetKey(Key key) => Application.Window.handle.GetKey(key);
    public static bool GetKeyDown(Key key) => Application.Window.handle.GetKeyDown(key);
    public static bool GetKeyUp(Key key) => Application.Window.handle.GetKeyUp(key);

    public static Vector2 GetMouseAxis() => Application.Window.handle.GetMouseAxis();

    public static bool GetMouseButtonDown(MouseButton mouseButton) =>
        Application.Window.handle.GetMouseButtonDown(mouseButton);

    public static bool GetMouseButtonUp(MouseButton mouseButton) =>
        Application.Window.handle.GetMouseButtonUp(mouseButton);
}
