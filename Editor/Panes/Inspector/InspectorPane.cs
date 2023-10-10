namespace Rin.Editor.Panes.Inspector;

sealed class InspectorPane : Pane {
    public InspectorPane() : base("Inspector") {
    }

    protected override void OnRender() {
        new InspectorHeaderView(Gui.Project.Tags.ToArray(), Gui.Project.Tags.ToArray()).Render();
        new InspectorView().Render();
    }
}