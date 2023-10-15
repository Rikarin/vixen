using Arch.Core;
using Rin.Core.Components;
using Rin.Core.General;
using Serilog;
using System.Drawing;
using System.Numerics;

namespace Rin.Editor; 

public class EditorCamera : Camera, IScript {
    Size viewportSize;

    Matrix4x4 viewMatrix;
    
    public Entity Entity { get; private set; }
    
    // TODO: use LocalTransform?
    // public Vector3 Position { get; private set; }
    
    public float VerticalFov { get; private set; }
    public float AspectRatio { get; private set; }
    public float NearClip { get; private set; }
    public float FarClip { get; private set; }
    public float Pitch { get; private set; }
    public float Yaw { get; private set; }
    public float CameraSpeed { get; private set; }

    public Matrix4x4 ViewProjection => Projection * viewMatrix;

    public EditorCamera(float degFieldOfView, Size size, float nearPlane, float farPlane) {
        VerticalFov = ToRadians(degFieldOfView);
        NearClip = nearPlane;
        FarClip = farPlane;
        // SetPerspective(VerticalFov, size, nearPlane, farPlane);
        SetViewportSize(size);
    }


    public void SetViewportSize(Size size) {
        if (size != viewportSize) {
            SetPerspective(VerticalFov, size, NearClip, FarClip);
            // SetOrthographic(size, NearClip, FarClip);
            viewportSize = size;
        }
    }



    public void OnStart() {
        var pos = SceneManager.ActiveScene.World.Get<LocalToWorld>(Entity);
        Matrix4x4.Invert(pos.Value, out viewMatrix);
    }

    public void OnUpdate() {
        var pos = SceneManager.ActiveScene.World.Get<LocalToWorld>(Entity);
        Matrix4x4.Invert(pos.Value, out viewMatrix);
        
        Log.Information("View: {Variable}", viewMatrix);
        Log.Information("Projection: {Variable}", Projection);
        Log.Information("View Projection: {Variable}", ViewProjection);
        // Quaternion.CreateFromYawPitchRoll()
    }

    public void Initialize(Entity entity) {
        Entity = entity;
        OnStart();
    }
    //
    // Vector3 CalculatePosition() {
    //     
    // }
    
    float ToRadians(float degrees) => degrees * (MathF.PI / 180);
    
}
