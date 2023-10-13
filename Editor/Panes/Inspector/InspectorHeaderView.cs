using Rin.Core.UI;

namespace Rin.Editor.Panes.Inspector;

public class InspectorHeaderView : View {
    readonly string[] tags;
    readonly string[] layers;
    readonly InspectorHeaderData data;
    
    // @formatter:off
    protected override View Body =>
        VStack(
            HStack(
                Toggle(data.IsEnabled),
                TextField(data.Name),
                TextField(data.EntityId),
                Toggle("Static", data.IsStatic)
            ),
            Grid(
                GridRow(
                    Picker("Tag", data.Tag, tags),
                    Picker("Layer", data.Layer, layers)
                )
            )
        );
    // @formatter:on

    public InspectorHeaderView(InspectorHeaderData data, string[] tags, string[] layers) {
        this.data = data;
        this.tags = tags;
        this.layers = layers;
    }
}