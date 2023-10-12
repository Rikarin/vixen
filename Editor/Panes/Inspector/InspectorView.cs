using Rin.Core.UI;

namespace Rin.Editor.Panes.Inspector;

public class InspectorView : View {
    readonly View[] views;

    public InspectorView(View[] views) {
        this.views = views;
    }
    
    // @formatter:off
    protected override View Body =>
        VStack(
            Spacing(),
            Spacing(),
            VStack(views)
        );
    // formatter:on
}
