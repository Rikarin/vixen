using System.Numerics;

namespace Rin.Core.Components;

public record struct LocalTransform(
    Vector3 Position,
    Quaternion Rotation,
    float Scale
);
