using Vixen.UI;

namespace Vixen.Editor.Panes.Inspector;

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
                            Text("X"),
                            Drag(data.Position.X)
                        ),
                        HStack(
                            Text("Y"),
                            Drag(data.Position.Y)
                        ),
                        HStack(
                            Text("Z"),
                            Drag(data.Position.Z)
                        )
                    )
                ),
                Grid(
                    GridRow(
                        Text("Rotation"),
                        HStack(
                            Text("X"),
                            Drag(data.Rotation.X)
                        ),
                        HStack(
                            Text("Y"),
                            Drag(data.Rotation.Y)
                        ),
                        HStack(
                            Text("Z"),
                            Drag(data.Rotation.Z)
                        ),
                        HStack(
                            Text("W"),
                            Drag(data.Rotation.W)
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