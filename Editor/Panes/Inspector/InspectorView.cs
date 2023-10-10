using Rin.Core.UI;

namespace Rin.Editor.Panes.Inspector;

public class InspectorView : View {
    // @formatter:off
    protected override View Body =>
        VStack(
            Spacing(),
            Spacing(),
            new TransformView(),
            new MeshFilterView()
        );
    // formatter:on
}
