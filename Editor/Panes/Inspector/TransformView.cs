using Rin.Core.UI;

namespace Rin.Editor.Panes.Inspector;

public class TransformView : View {
    public class RowData {
        public string Name { get; set; }
        public State<float> X { get; } = new();
        public State<float> Y { get; } = new();
        public State<float> Z { get; } = new();
    }

    // TODO: not static
    static RowData[] data = {
        new() { Name = "Position" },
        new() { Name = "Rotation" },
        new() { Name = "Scale" }
    };
    
    // @formatter:off
    protected override View Body =>
        CollapsingView("Transform",
            Table(data,
                TableColumn<RowData>("Name", x => x.Name),
                TableColumn<RowData>("X", data => 
                    HStack(
                        Drag(data.X),
                        Text("X")
                    )
                ),
                TableColumn<RowData>("Y", data => 
                    HStack(
                        Drag(data.Y),
                        Text("Y")
                    )
                ),
                TableColumn<RowData>("Z", data => 
                    HStack(
                        Drag(data.Z),
                        Text("Z")
                    )
                )
            ).Style(new TableStyle { HasHeader = false })
        );
    // @formatter:on
}
