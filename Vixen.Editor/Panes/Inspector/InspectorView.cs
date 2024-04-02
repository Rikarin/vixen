using Vixen.UI;

namespace Vixen.Editor.Panes.Inspector;

public class InspectorView : View {
    readonly View[] views;

    public InspectorView(View[] views) {
        this.views = views;
    }
    
    // @formatter:off
    protected override View Body =>
        VStack(
            ForEach(views, view => VStack(
                Spacing(),
                Spacing(),
                Spacing(),
                Spacing(),
                view
            )),
            
            Spacing(),
            Spacing(),
            Spacing(),
            Spacing(),
            Spacing(),
            Spacing(),
            Spacing(),
            Button("Add Component")
        );
    // formatter:on
}
