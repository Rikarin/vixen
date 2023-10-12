using Rin.Core.UI;
using Rin.Editor.States;

namespace Rin.Editor.Panes.Inspector;

public class TransformView : View {
    readonly TransformViewData data;

    public TransformView(TransformViewData data) {
        this.data = data;
    }
    
    // @formatter:off
    protected override View Body =>
        CollapsingView("Local Transform",
            VStack(
                Grid(
                    GridRow(
                        Text("Position"),
                        HStack(
                            Drag(data.Position.X),
                            Text("X")
                        ),
                        HStack(
                            Drag(data.Position.Y),
                            Text("Y")
                        ),
                        HStack(
                            Drag(data.Position.Z),
                            Text("Z")
                        )
                    )
                ),
                Grid(
                    GridRow(
                        Text("Rotation"),
                        HStack(
                            Drag(data.Rotation.X),
                            Text("X")
                        ),
                        HStack(
                            Drag(data.Rotation.Y),
                            Text("Y")
                        ),
                        HStack(
                            Drag(data.Rotation.Z),
                            Text("Z")
                        ),
                        HStack(
                            Drag(data.Rotation.W),
                            Text("W")
                        )
                    )
                ),
                Grid(
                    GridRow(
                        Text("Scale"),
                        Drag(data.Scale)
                    )
                )
            )
        );
    // @formatter:on
}

public class TransformViewData {
    public Vector3State Position { get; } = new();
    public QuaternionState Rotation { get; } = new();
    public State<float> Scale { get; } = new();
}