using System.Numerics;

namespace Rin.Core.Math; 

public static class Matrix {
    public static Matrix4x4 TRS(Vector3 position, Quaternion rotation, Vector3 scale) =>
        Matrix4x4.Identity
        * Matrix4x4.CreateScale(scale)
        * Matrix4x4.CreateFromQuaternion(rotation)
        * Matrix4x4.CreateTranslation(position);
}
