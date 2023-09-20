using Rin.Core.Math;
using System.Numerics;

namespace Rin.Core.General;

public class Transform : Component {
    public Transform? Parent { get; set; }


    public Vector3 Position { get; private set; }
    public Quaternion Rotation { get; private set; }

    public Vector3 EulerAngles => Rotation.ToEulerAngles();


    public Vector3 LocalPosition { get; set; }
    public Quaternion LocalRotation { get; set; }
    public Vector3 LocalScale { get; set; }
    public Vector3 LocalEulerAngles => LocalRotation.ToEulerAngles();

    public Transform Root => Parent != null ? Parent.Root : this;

    public Matrix4x4 ViewMatrix =>
        Matrix4x4.Identity
        * Matrix4x4.CreateScale(LocalScale)
        * Matrix4x4.CreateFromQuaternion(LocalRotation)
        * Matrix4x4.CreateTranslation(LocalPosition);


    public void SetParent(Transform? parent, bool sameWorldPosition = false) {
        Parent = parent;
    }
}
