using System.Numerics;

namespace Vixen.InputSystem;

/// <summary>
///     This class represents a sensor of type Accelerometer. It measures the acceleration forces (including gravity)
///     applying on the device.
/// </summary>
public interface IAccelerometerSensor : ISensorDevice {
    /// <summary>
    ///     Gets the current acceleration applied on the device (in meters/seconds^2).
    /// </summary>
    Vector3 Acceleration { get; }
}
