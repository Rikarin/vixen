using ImGuiNET;
using Rin.Core.UI;

namespace Rin.Editor.Panes;

sealed class InspectorPane : Pane {
    readonly TransformView transformView = new();
    readonly MeshFilterView meshFilterView = new();

    public InspectorPane() : base("Inspector") {
        // position = transform.LocalPosition;
        // rotation = transform.LocalRotation.ToEulerAngles();
        // scale = transform.LocalScale;
    }

    protected override void OnRender() {
        new InspectorHeaderView(Gui.Project.Tags.ToArray(), Gui.Project.Tags.ToArray()).Render();
        ImGui.Spacing();
        ImGui.Spacing();
        transformView.Render();
        meshFilterView.Render();
        
        ImGui.ShowMetricsWindow();
    }
}

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
                    Picker("Tag", data.Layer, layers)
                )
            )
        );
    // @formatter:on
}
