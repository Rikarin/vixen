using Rin.Core.UI;

namespace Rin.Editor.Panes.Inspector;

public class InspectorHeaderView : View {
    readonly string[] tags;
    readonly string[] layers;

    public class InspectorHeaderData {
        public State<bool> IsEnabled { get; set; } = new();
        public State<string> Name { get; set; } = new();
        public State<bool> IsStatic { get; set; } = new();
        
        public State<string> Tag { get; set; } = new();
        public State<string> Layer { get; set; } = new();
    }

    // TODO
    static InspectorHeaderData data = new();

    public InspectorHeaderView(string[] tags, string[] layers) {
        this.tags = tags;
        this.layers = layers;
    }
    
    // @formatter:off
    protected override View Body =>
        VStack(
            HStack(
                Toggle(data.IsEnabled),
                TextField(data.Name),
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
}
