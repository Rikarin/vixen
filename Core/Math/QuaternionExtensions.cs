using System.Numerics;

namespace Rin.Core.Math;

public static class QuaternionExtensions {
    public static Vector3 ToEulerAngles(this Quaternion q) {
        Vector3 angles = new();

        // roll / x
        double sinrCosp = 2 * (q.W * q.X + q.Y * q.Z);
        double cosrCosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
        angles.X = (float)System.Math.Atan2(sinrCosp, cosrCosp);

        // pitch / y
        double sinp = 2 * (q.W * q.Y - q.Z * q.X);
        if (System.Math.Abs(sinp) >= 1) {
            angles.Y = (float)System.Math.CopySign(System.Math.PI / 2, sinp);
        } else {
            angles.Y = (float)System.Math.Asin(sinp);
        }

        // yaw / z
        double sinyCosp = 2 * (q.W * q.Z + q.X * q.Y);
        double cosyCosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
        angles.Z = (float)System.Math.Atan2(sinyCosp, cosyCosp);

        return angles;
    }

    public static Quaternion ToQuaternion(this Vector3 v) {
        var cy = (float)System.Math.Cos(v.Z * 0.5);
        var sy = (float)System.Math.Sin(v.Z * 0.5);
        var cp = (float)System.Math.Cos(v.Y * 0.5);
        var sp = (float)System.Math.Sin(v.Y * 0.5);
        var cr = (float)System.Math.Cos(v.X * 0.5);
        var sr = (float)System.Math.Sin(v.X * 0.5);

        return new() {
            W = cr * cp * cy + sr * sp * sy,
            X = sr * cp * cy - cr * sp * sy,
            Y = cr * sp * cy + sr * cp * sy,
            Z = cr * cp * sy - sr * sp * cy
        };
    }
}
