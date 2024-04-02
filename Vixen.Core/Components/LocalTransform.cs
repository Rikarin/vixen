using System.Numerics;

namespace Vixen.Core.Components;

public record struct LocalTransform(
    Vector3 Position,
    Quaternion Rotation,
    float Scale
) {
    public Vector3 Right => Vector3.Normalize(Vector3.Transform(new(1, 0, 0), Rotation));
    public Vector3 Up => Vector3.Normalize(Vector3.Transform(new(0, 1, 0), Rotation));
    public Vector3 Forward => Vector3.Normalize(Vector3.Transform(new(0, 0, 1), Rotation));
}
