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
                        Drag(row.X),
                        Text("X")
                    )
                ),
                TableColumn<Vector4State>("Y", row => 
                    HStack(
                        Drag(row.Y),
                        Text("Y")
                    )
                ),
                TableColumn<Vector4State>("Z", row => 
                    HStack(
                        Drag(row.Z),
                        Text("Z")
                    )
                ),
                TableColumn<Vector4State>("W", row => 
                    HStack(
                        Drag(row.W),
                        Text("W")
                    )
                )
            ).Style(new TableStyle { HasHeader = false })
        );
    // @formatter:on
}
