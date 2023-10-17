using System.Numerics;

namespace Rin.Core.Math;

public static class Matrix {
    public static Matrix4x4 TRS(in Vector3 position, in Quaternion rotation, in Vector3 scale) =>
        Matrix4x4.CreateScale(scale)
        * Matrix4x4.CreateFromQuaternion(rotation)
        * Matrix4x4.CreateTranslation(position);
}