using System.Drawing;
using System.Numerics;

namespace Vixen.Core.Components;

public abstract class Camera {
    public Matrix4x4 Projection { get; protected set; } = Matrix4x4.Identity;
    public float Exposure { get; protected set; }

    public Camera() { }

    public Camera(float fieldOfView, SizeF size, float nearPlane, float farPlane) {
        SetPerspective(fieldOfView, size, nearPlane, farPlane);
    }

    public void SetPerspective(float fieldOfView, SizeF size, float nearPlane, float farPlane) {
        // TODO: Verify this (near/far planes)
        Projection = Matrix4x4.CreatePerspectiveFieldOfView(fieldOfView, size.Width / size.Height, nearPlane, farPlane);
    }

    public void SetOrthographic(SizeF size, float nearPlane, float farPlane) {
        // TODO: Verify this (near/far planes)
        Projection = Matrix4x4.CreateOrthographic(size.Width, size.Height, nearPlane, farPlane);
    }
}
