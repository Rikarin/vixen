using System.Numerics;

namespace Rin.Core.General;

public abstract class Camera : Behaviour {
    public static Camera Main;
    public Matrix4x4 Projection { get; protected set; } = Matrix4x4.Identity;

    // TODO: finish this
}
