using Rin.Core.UI;
using System.Numerics;

namespace Rin.Editor.States;

public class Vector3State : State<Vector3> {
    public State<float> X { get; } = new();
    public State<float> Y { get; } = new();
    public State<float> Z { get; } = new();

    public Vector3State() {
        X.Subscribe(OnChanged);
        Y.Subscribe(OnChanged);
        Z.Subscribe(OnChanged);
    }

    public override void SetNext(Vector3 value, bool sendEvent = true) {
        base.SetNext(value);

        X.SetNext(value.X, false);
        Y.SetNext(value.Y, false);
        Z.SetNext(value.Z, false);
    }

    void OnChanged(float _) {
        SetNext(new(X.Value, Y.Value, Z.Value));
    }
}