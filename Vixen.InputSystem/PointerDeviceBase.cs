using System.Numerics;

namespace Vixen.InputSystem;

/// <summary>
///     Base class for pointer devices
/// </summary>
public abstract class PointerDeviceBase : IPointerDevice {
    protected PointerDeviceState PointerState;

    public Vector2 SurfaceSize => PointerState.SurfaceSize;
    public float SurfaceAspectRatio => PointerState.SurfaceAspectRatio;
    public IReadOnlySet<PointerPoint> PressedPointers => PointerState.PressedPointers;
    public IReadOnlySet<PointerPoint> ReleasedPointers => PointerState.ReleasedPointers;
    public IReadOnlySet<PointerPoint> DownPointers => PointerState.DownPointers;

    public int Priority { get; set; }
    public abstract string Name { get; }
    public abstract Guid Id { get; }
    public abstract IInputSource Source { get; }

    protected PointerDeviceBase() {
        PointerState = new(this);
    }

    public virtual void Update(List<InputEvent> inputEvents) {
        PointerState.Update(inputEvents);
    }

    public event EventHandler<SurfaceSizeChangedEventArgs> SurfaceSizeChanged;

    /// <summary>
    ///     Calls <see cref="PointerDeviceState.SetSurfaceSize" /> and invokes the <see cref="SurfaceSizeChanged" /> event
    /// </summary>
    /// <param name="newSize">New size of the surface</param>
    protected void SetSurfaceSize(Vector2 newSize) {
        PointerState.SetSurfaceSize(newSize);
        SurfaceSizeChanged?.Invoke(this, new() { NewSurfaceSize = newSize });
    }

    protected Vector2 Normalize(Vector2 position) => position * PointerState.InverseSurfaceSize;
}
