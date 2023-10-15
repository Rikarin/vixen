using Arch.Core;
using Arch.Core.Extensions;
using Rin.Core.Abstractions;
using Rin.Core.Components;
using Rin.Core.General;
using Serilog;
using System.Drawing;
using System.Numerics;

namespace Rin.Editor;

public class EditorCamera : Camera, IScript {
    Size viewportSize;
    Matrix4x4 viewMatrix;
    Vector2 previousMousePosition;

    Vector3 positionDelta;
    float pitchDelta;
    float yawDelta;

    public Entity Entity { get; private set; }
    public float VerticalFov { get; private set; }

    // public float AspectRatio { get; private set; }
    public float NearClip { get; private set; }
    public float FarClip { get; private set; }
    public Vector3 FocalPoint { get; private set; }
    public float Distance { get; private set; }
    public Vector3 Direction { get; private set; }

    public Matrix4x4 ViewProjection => viewMatrix * Projection;
    public float ZoomSpeed => MathF.Min(MathF.Pow(MathF.Max(Distance * 0.2f, 0), 2), 50f);
    float RotationSpeed => 0.3f;

    Vector2 PanSpeed {
        get {
            var x = MathF.Min(viewportSize.Width / 1000f, 2.4f);
            var xFactor = 0.0366f * (x * x) - 0.1778f * x + 0.3021f;

            var y = MathF.Min(viewportSize.Height / 1000f, 2.4f);
            var yFactor = 0.0366f * (y * y) - 0.1778f * y + 0.3021f;

            return new(xFactor, yFactor);
        }
    }

    public EditorCamera(float degFieldOfView, Size size, float nearPlane, float farPlane) {
        VerticalFov = ToRadians(degFieldOfView);
        NearClip = nearPlane;
        FarClip = farPlane;
        SetViewportSize(size);
    }

    public void SetViewportSize(Size size) {
        if (size != viewportSize) {
            SetPerspective(VerticalFov, size, NearClip, FarClip);
            viewportSize = size;
        }
    }

    public void OnStart() {
        var pos = SceneManager.ActiveScene.World.Get<LocalToWorld>(Entity);
        Matrix4x4.Invert(pos.Value, out viewMatrix);
    }

    public void OnUpdate() {
        ref var transform = ref Entity.Get<LocalTransform>();

        var mousePos = Input.MousePosition;
        var deltaMouse = (mousePos - previousMousePosition) * 0.002f;
        previousMousePosition = mousePos;

        if (Input.GetKey(Key.ShiftLeft)) {
            if (Input.GetMouseButton(MouseButton.Middle)) {
                MousePan(deltaMouse);
            }
        } else {
            if (Input.GetMouseButton(MouseButton.Middle)) {
                MouseRotate(deltaMouse);
            }
        }

        // TODO: scroll
        if (Input.GetMouseButton(MouseButton.Left)) {
            var axis = Input.GetMouseAxis();
            MouseZoom(axis.X * 0.1f * Time.DeltaTime);
        }

        var testFly = false;
        // Fly Cam
        if (Input.GetMouseButton(MouseButton.Right)) {
            testFly = true;
            var yawSign = transform.Up.Y < 0 ? -1f : 1f;
            var speed = GetCameraSpeed();
            var upDirection = new Vector3(0, yawSign, 0);
            var rightDirection = Vector3.Cross(Direction, upDirection);

            if (Input.GetKey(Key.W)) {
                positionDelta += Direction * speed * Time.DeltaTime;
            }

            if (Input.GetKey(Key.S)) {
                positionDelta -= Direction * speed * Time.DeltaTime;
            }
            
            if (Input.GetKey(Key.A)) {
                positionDelta -= rightDirection * speed * Time.DeltaTime;
            }
            
            if (Input.GetKey(Key.D)) {
                positionDelta += rightDirection * speed * Time.DeltaTime;
            }
            
            if (Input.GetKey(Key.Q)) {
                positionDelta -= upDirection * speed * Time.DeltaTime;
            }
            
            if (Input.GetKey(Key.E)) {
                positionDelta += upDirection * speed * Time.DeltaTime;
            }

            const float maxRate = 0.12f;
            yawDelta += Math.Clamp(yawSign * deltaMouse.X * RotationSpeed, -maxRate, maxRate);
            pitchDelta += Math.Clamp(deltaMouse.Y * RotationSpeed, -maxRate, maxRate);
            Direction = Vector3.Transform(
                Direction,
                Quaternion.Normalize(
                    Quaternion.CreateFromAxisAngle(rightDirection, -pitchDelta)
                    * Quaternion.CreateFromAxisAngle(upDirection, -yawDelta)
                )
            );
            Distance = Vector3.Distance(FocalPoint, transform.Position);
            FocalPoint = transform.Position + transform.Forward * Distance;
        }

        // apply smoothing
        transform.Position += positionDelta;
        transform.Rotation *= Quaternion.CreateFromYawPitchRoll(yawDelta, pitchDelta, 0);

        // If arcball
        if (!testFly) {
            transform.Position = FocalPoint - transform.Forward * Distance + positionDelta;
        }

        UpdateCameraView();
    }

    public void Initialize(Entity entity) {
        Entity = entity;
        OnStart();
    }

    // void DisableMouse() {
    //     // SilkWindow.MainWindow.silkWindow.inp
    // }

    void UpdateCameraView() {
        ref var transform = ref Entity.Get<LocalTransform>();

        var yawSign = transform.Up.Y < 0 ? -1f : 1f;
        var cosAngle = Vector3.Dot(transform.Forward, transform.Up);
        if (cosAngle * yawSign > 0.99f) {
            pitchDelta = 0;
        }

        var lookAt = transform.Position + transform.Forward;
        Direction = Vector3.Normalize(lookAt - transform.Position);
        Distance = Vector3.Distance(transform.Position, FocalPoint);
        viewMatrix = Matrix4x4.CreateLookAt(transform.Position, lookAt, new(0, yawSign, 0));

        yawDelta *= 0.6f;
        pitchDelta *= 0.6f;
        positionDelta *= 0.8f;
    }

    void MousePan(Vector2 delta) {
        ref var transform = ref Entity.Get<LocalTransform>();
        // TODO: this was -
        FocalPoint += transform.Right * delta.X * PanSpeed.X * Distance;
        FocalPoint += transform.Up * delta.Y * PanSpeed.Y * Distance;
    }

    void MouseRotate(Vector2 delta) {
        ref var transform = ref Entity.Get<LocalTransform>();
        // TODO
        var yawSign = transform.Up.Y < 0 ? 1f : -1f;

        yawDelta += yawSign * delta.X * RotationSpeed;
        pitchDelta += delta.Y * RotationSpeed;
    }

    void MouseZoom(float delta) {
        ref var transform = ref Entity.Get<LocalTransform>();
        Distance -= delta * ZoomSpeed;
        var forwardDir = transform.Forward;
        transform.Position = FocalPoint - forwardDir * Distance;

        if (Distance < 1) {
            FocalPoint += forwardDir * Distance;
            Distance = 1;
        }

        positionDelta += delta * ZoomSpeed * forwardDir;
    }

    float GetCameraSpeed() {
        var speed = 0.2f;
        if (Input.GetKey(Key.ControlLeft)) {
            speed /= 2 - MathF.Log(speed);
        }
        
        if (Input.GetKey(Key.ShiftLeft)) {
            speed *= 2 - MathF.Log(speed);
        }

        return Math.Clamp(speed, 0.0005f, 2.0f);
    }

    float ToRadians(float degrees) => degrees * (MathF.PI / 180);
}
