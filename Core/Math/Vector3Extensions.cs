using System.Numerics;

namespace Rin.Core.Math;

public static class Vector3Extensions {
    public static Vector3 ToDegrees(this Vector3 radians) {
        return new(Deg(radians.X), Deg(radians.Y), Deg(radians.Z));
        float Deg(float value) => value * (180 / MathF.PI);
    }
    
    public static Vector3 ToRadians(this Vector3 degrees) {
        return new(Rad(degrees.X), Rad(degrees.Y), Rad(degrees.Z));
        float Rad(float value) => value * (MathF.PI / 180);
    }
}
