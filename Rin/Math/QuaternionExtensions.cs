using System.Numerics;

namespace Editor.Math;

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
}
