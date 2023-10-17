using System.Numerics;

namespace Rin.InputSystem;

/// <summary>
///     This class represents a sensor of type Gravity. It measures the gravity force applying on the device.
/// </summary>
public interface IGravitySensor : ISensorDevice {
    /// <summary>
    ///     Gets the current gravity applied on the device (in meters/seconds^2).
    /// </summary>
    Vector3 Vector { get; }
}
