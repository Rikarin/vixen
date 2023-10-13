using Arch.Core;
using Rin.Core.Components;

namespace Rin.Editor; 

public class EditorCamera : Camera, IScript {
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
    
    public void OnStart() { }

    public void OnUpdate() {
        // Quaternion.CreateFromYawPitchRoll()
    }

    public void Initialize(Entity entity) {
        Entity = entity;
        OnStart();
    }
}
