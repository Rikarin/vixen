using Rin.Core.UI;
using Rin.Editor.States;

namespace Rin.Editor.Panes.Inspector;

public class TransformViewData {
    public Vector3State Position { get; } = new();
    public QuaternionState Rotation { get; } = new();
    // public Vector3State Rotation { get; } = new();
    public State<float> Scale { get; } = new();
    
    public bool IsDirty { get; private set; }

    public TransformViewData() {
        Position.Subscribe(_ => IsDirty = true);
        Rotation.Subscribe(_ => IsDirty = true);
        Scale.Subscribe(_ => IsDirty = true);
    }

    public void ResetDirty() {
        IsDirty = false;
    }
}
