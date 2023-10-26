using Rin.InputSystem;
using Silk.NET.Input;
using Silk.NET.Maths;
using System.Numerics;
using ISilkMouse = Silk.NET.Input.IMouse;
using SilkMouseButton = Silk.NET.Input.MouseButton;
using ISilkWindow = Silk.NET.Windowing.IWindow;
using MouseButton = Rin.InputSystem.MouseButton;

namespace Rin.Platform.Silk.Input;

public class SilkMouse : MouseDeviceBase, IDisposable {
    readonly ISilkMouse silkMouse;
    readonly ISilkWindow silkWindow;
    bool isPositionLocked;

    public override string Name { get; }
    public override Guid Id { get; }
    public override IInputSource Source { get; }
    public override bool IsPositionLocked => isPositionLocked;

    public SilkMouse(IInputSource inputSource, ISilkMouse silkMouse, ISilkWindow silkWindow) {
        this.silkMouse = silkMouse;
        this.silkWindow = silkWindow;
        Source = inputSource;

        // TODO
        Id = new("070B2C18-8502-458C-A403-334C7AF95241");
        Name = silkMouse.Name;

        silkWindow.Resize += OnResize;
        silkMouse.MouseDown += OnMouseDown;
        silkMouse.MouseUp += OnMouseUp;
        silkMouse.MouseMove += OnMouseMove;
        silkMouse.Scroll += OnScroll;

        OnResize(silkWindow.Size);
    }

    public override void UnlockPosition() {
        silkMouse.Cursor.CursorMode = CursorMode.Normal;
        isPositionLocked = false;
    }

    public override void SetPosition(Vector2 normalizedPosition) {
        silkMouse.Position = normalizedPosition;
    }

    public override void LockPosition(bool forceCenter = false) {
        silkMouse.Cursor.CursorMode = forceCenter ? CursorMode.Disabled : CursorMode.Hidden;
        isPositionLocked = true;
    }

    public void Dispose() {
        silkWindow.Resize -= OnResize;
        silkMouse.MouseDown -= OnMouseDown;
        silkMouse.MouseUp -= OnMouseUp;
        silkMouse.MouseMove -= OnMouseMove;
        silkMouse.Scroll -= OnScroll;
    }

    void OnResize(Vector2D<int> size) => SetSurfaceSize(new(size.X, size.Y));
    void OnScroll(ISilkMouse _, ScrollWheel scrollWheel) => MouseState.HandleMouseWheel(scrollWheel.Y);
    void OnMouseMove(ISilkMouse _, Vector2 position) => MouseState.HandleMove(position);
    void OnMouseUp(ISilkMouse _, SilkMouseButton button) => MouseState.HandleButtonUp(MapButton(button));
    void OnMouseDown(ISilkMouse _, SilkMouseButton button) => MouseState.HandleButtonDown(MapButton(button));

    MouseButton MapButton(SilkMouseButton silkMouseButton) =>
        silkMouseButton switch {
            SilkMouseButton.Left => MouseButton.Left,
            SilkMouseButton.Middle => MouseButton.Middle,
            SilkMouseButton.Right => MouseButton.Right,
            // SilkMouseButton.Unknown => expr,
            // SilkMouseButton.Button4 => expr,
            // SilkMouseButton.Button5 => expr,
            // SilkMouseButton.Button6 => expr,
            // SilkMouseButton.Button7 => expr,
            // SilkMouseButton.Button8 => expr,
            // SilkMouseButton.Button9 => expr,
            // SilkMouseButton.Button10 => expr,
            // SilkMouseButton.Button11 => expr,
            // SilkMouseButton.Button12 => expr,
            _ => throw new ArgumentOutOfRangeException(nameof(silkMouseButton), silkMouseButton, null)
        };
}
