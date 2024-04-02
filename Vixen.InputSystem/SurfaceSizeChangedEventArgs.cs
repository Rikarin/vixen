using System.Numerics;

namespace Vixen.InputSystem;

/// <summary>
///     An event for when the size of a pointer surface changed
/// </summary>
public class SurfaceSizeChangedEventArgs : EventArgs {
    /// <summary>
    ///     The new size of the surface
    /// </summary>
    public Vector2 NewSurfaceSize { get; internal set; }
}
