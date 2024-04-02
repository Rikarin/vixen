using System.Numerics;

namespace Vixen.InputSystem;

/// <summary>
///     Represents a unique pointer that is or was on the screen and information about it
/// </summary>
public class PointerPoint {
    /// <summary>
    ///     Last position of the pointer
    /// </summary>
    public Vector2 Position { get; internal set; }

    /// <summary>
    ///     Pointer delta
    /// </summary>
    public Vector2 Delta { get; internal set; }

    /// <summary>
    ///     Is the pointer currently down
    /// </summary>
    public bool IsDown { get; internal set; }

    /// <summary>
    ///     The pointer ID, from the device
    /// </summary>
    public int Id { get; internal set; }

    /// <summary>
    ///     The device to which this pointer belongs
    /// </summary>
    public IPointerDevice Pointer { get; internal set; }

    public override string ToString() =>
        $"Pointer [{Id}] {nameof(Position)}: {Position}, {nameof(Delta)}: {Delta}, {nameof(IsDown)}: {IsDown}, {nameof(Pointer)}: {Pointer}";
}
