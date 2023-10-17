using Arch.Core;
using Arch.Core.Extensions;
using Rin.Core.Components;
using Rin.Core.General;
using Rin.InputSystem;
using Rin.InputSystem.VirtualButtons;
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

    protected InputManager Input => InputContainer.inputManager;
    public Entity Entity { get; private set; }
    public EditorCameraMode Mode { get; private set; }
    public float VerticalFov { get; private set; }

    // public float AspectRatio { get; private set; }
    public float NearClip { get; private set; }
    public float FarClip { get; private set; }
    public Vector3 FocalPoint { get; private set; }
    public float Distance { get; private set; }
    public Vector3 Direction { get; private set; }

    public Matrix4x4 ViewProjection => viewMatrix * Projection;
    public float ZoomSpeed => MathF.Min(MathF.Pow(MathF.Max(Distance * 0.02f, 0), 2), 2f);
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

        var shift = new VirtualButtonConfig {
            new("Shift", VirtualButton.Keyboard.LeftShift),
            new("Shift", VirtualButton.Keyboard.RightShift),
        };

        Input.VirtualButtonConfigSet ??= new();
        Input.VirtualButtonConfigSet.Add(shift);
    }

    public void OnUpdate() {
        ref var transform = ref Entity.Get<LocalTransform>();
        var mousePos = Input.AbsoluteMousePosition;
        
        // TODO
        var deltaMouse = (mousePos - previousMousePosition) * 0.002f;
        previousMousePosition = mousePos;

        // Log.Information("Mouse Pos: {Variable}", mousePos);
        // Log.Information("Debug: {Variable} {a}", Input.MouseDelta, Input.AbsoluteMouseDelta);

        if (Input.GetVirtualButton(0, "Shift") > 0) {
            Log.Information("foo bar");
        }

        if (Input.MouseWheelDelta is > 0 or < 0) {
            MouseZoom(Input.MouseWheelDelta);
        }
        
        if (Input.IsMouseButtonDown(MouseButton.Right)) {
            DisableMouse();
            Mode = EditorCameraMode.FlyCam;
            var yawSign = transform.Up.Y < 0 ? -1f : 1f;
            var speed = GetCameraSpeed();
            var upDirection = new Vector3(0, yawSign, 0);
            var rightDirection = Vector3.Cross(Direction, upDirection);
        
            if (Input.IsKeyDown(Key.W)) {
                positionDelta += Direction * speed * Time.DeltaTime;
            }
        
            if (Input.IsKeyDown(Key.S)) {
                positionDelta -= Direction * speed * Time.DeltaTime;
            }
            
            if (Input.IsKeyDown(Key.A)) {
                positionDelta -= rightDirection * speed * Time.DeltaTime;
            }
            
            if (Input.IsKeyDown(Key.D)) {
                positionDelta += rightDirection * speed * Time.DeltaTime;
            }
            
            if (Input.IsKeyDown(Key.Q)) {
                positionDelta -= upDirection * speed * Time.DeltaTime;
            }
            
            if (Input.IsKeyDown(Key.E)) {
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
        } else if (Input.IsMouseButtonDown(MouseButton.Middle)) {
            Mode = EditorCameraMode.ArcBall;
            DisableMouse();
        
            if (Input.IsKeyDown(Key.LeftShift)) {
                MousePan(deltaMouse);
            } else {
                MouseRotate(deltaMouse);
            } 
        } else {
            EnableMouse();
        }

        // apply smoothing
        transform.Position += positionDelta;
        transform.Rotation *= Quaternion.CreateFromYawPitchRoll(yawDelta, pitchDelta, 0);

        if (Mode == EditorCameraMode.ArcBall) {
            transform.Position = FocalPoint - transform.Forward * Distance + positionDelta;
        }

        UpdateCameraView();
    }

    public void Initialize(Entity entity) {
        Entity = entity;
        OnStart();
    }

    void DisableMouse() {
        Input.LockMousePosition(true);
    }

    void EnableMouse() {
        Input.UnlockMousePosition();
    }

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
        if (Input.IsKeyDown(Key.LeftCtrl)) {
            speed /= 2 - MathF.Log(speed);
        }
        
        if (Input.IsKeyDown(Key.LeftShift)) {
            speed *= 2 - MathF.Log(speed);
        }

        return Math.Clamp(speed, 0.0005f, 2.0f);
    }

    float ToRadians(float degrees) => degrees * (MathF.PI / 180);
}


public enum EditorCameraMode {
    FlyCam,
    ArcBall
}
