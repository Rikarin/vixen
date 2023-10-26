using System.Numerics;

namespace Rin.InputSystem;

/// <summary>
///     This class represents a sensor of type Gyroscope. It measures the rotation speed of device along the x/y/z axis.
/// </summary>
public interface IGyroscopeSensor : ISensorDevice {
    /// <summary>
    ///     Gets the current rotation speed of the device along x/y/z axis.
    /// </summary>
    Vector3 RotationRate { get; }
}
