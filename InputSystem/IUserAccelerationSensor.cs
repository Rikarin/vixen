using System.Numerics;

namespace Rin.InputSystem;

/// <summary>
///     This class represents a sensor of type user acceleration. It measures the acceleration applied by the user (no
///     gravity) onto the device.
/// </summary>
public interface IUserAccelerationSensor : ISensorDevice {
    /// <summary>
    ///     Gets the current acceleration applied by the user (in meters/seconds^2).
    /// </summary>
    Vector3 Acceleration { get; }
}
