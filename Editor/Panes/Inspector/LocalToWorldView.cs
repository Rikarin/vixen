using Rin.Core.UI;
using Rin.Editor.States;

namespace Rin.Editor.Panes.Inspector;

public class LocalToWorldView : View {
    readonly Vector4State[] rows;

    public LocalToWorldView(Vector4State[] rows) {
        this.rows = rows;
    }
    
    // @formatter:off
    protected override View Body =>
        CollapsingView("Local To World Matrix",
            Table(rows,
                TableColumn<Vector4State>("X", row => 
                    HStack(
                        Text("X"),
                        Drag(row.X)
                    )
                ),
                TableColumn<Vector4State>("Y", row => 
                    HStack(
                        Text("Y"),
                        Drag(row.Y)
                    )
                ),
                TableColumn<Vector4State>("Z", row => 
                    HStack(
                        Text("Z"),
                        Drag(row.Z)
                    )
                ),
                TableColumn<Vector4State>("W", row => 
                    HStack(
                        Text("W"),
                        Drag(row.W)
                    )
                )
            ).Style(new TableStyle { HasHeader = false })
        );
    // @formatter:on
}
