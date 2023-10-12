using Rin.Core.UI;

namespace Rin.Editor.Panes.Inspector;

public class GenericComponentView : View {
    readonly string name;

    public GenericComponentView(string name) {
        this.name = name;
    }
    
    // @formatter:off
    protected override View Body =>
        CollapsingView(name,
            VStack(
                Text("TODO")
            )
        );
    // @formatter:on
}
