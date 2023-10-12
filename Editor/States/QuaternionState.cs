using Rin.Core.UI;
using System.Numerics;

namespace Rin.Editor.States;

public class QuaternionState : State<Quaternion> {
    public State<float> X { get; } = new();
    public State<float> Y { get; } = new();
    public State<float> Z { get; } = new();
    public State<float> W { get; } = new();

    public QuaternionState() {
        X.Subscribe(OnChanged);
        Y.Subscribe(OnChanged);
        Z.Subscribe(OnChanged);
        W.Subscribe(OnChanged);
    }

    public override void SetNext(Quaternion value, bool sendEvent = true) {
        base.SetNext(value);

        X.SetNext(value.X, false);
        Y.SetNext(value.Y, false);
        Z.SetNext(value.Z, false);
        W.SetNext(value.W, false);
    }

    void OnChanged(float _) {
        SetNext(new(X.Value, Y.Value, Z.Value, W.Value));
    }
}