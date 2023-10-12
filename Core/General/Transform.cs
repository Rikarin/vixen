using Rin.Core.Math;
using System.Numerics;

namespace Rin.Core.General;

[Obsolete("Use LocalTransform")]
public class Transform : Component {
    Vector3 localPosition = new();
    Quaternion localRotation = new();
    Vector3 localScale = new();
    
    public Transform? Parent { get; set; }


    public Vector3 Position { get; private set; }
    public Quaternion Rotation { get; private set; }

    public Vector3 EulerAngles => Rotation.ToEulerAngles();

    public ref Vector3 LocalPosition => ref localPosition;
    public ref Quaternion LocalRotation => ref localRotation;
    public ref Vector3 LocalScale => ref localScale;
    public Vector3 LocalEulerAngles => LocalRotation.ToEulerAngles();

    public Transform Root => Parent != null ? Parent.Root : this;
    public Matrix4x4 ViewMatrix => Matrix.TRS(LocalPosition, LocalRotation, LocalScale);

    public void SetParent(Transform? parent, bool sameWorldPosition = false) {
        Parent = parent;
    }
}
