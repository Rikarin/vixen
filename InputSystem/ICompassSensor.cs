namespace Rin.InputSystem;

/// <summary>
///     This class represents a sensor of type compass. It measures the angle between the device and the north.
/// </summary>
public interface ICompassSensor : ISensorDevice {
    /// <summary>
    ///     Gets the value of north heading, that is the angle (in radian) between the top of the device and north.
    /// </summary>
    float Heading { get; }
}
