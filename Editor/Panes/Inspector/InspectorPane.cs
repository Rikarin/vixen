using Arch.Core;
using Rin.Core.Components;
using Rin.Core.General;
using Rin.Core.UI;
using Rin.Editor.States;

namespace Rin.Editor.Panes.Inspector;

sealed class InspectorPane : Pane {
    World World => SceneManager.ActiveScene!.World;
    Entity SelectedEntity => HierarchyPane.Selected!.Value;

    public InspectorPane() : base("Inspector") { }

    TransformViewData LoadTransform() {
        var data = new TransformViewData();

        var localTransform = World.Get<LocalTransform>(HierarchyPane.Selected.Value);
        data.Position.SetNext(localTransform.Position);
        data.Rotation.SetNext(localTransform.Rotation);
        data.Scale.SetNext(localTransform.Scale);

        return data;
    }

    void SaveTransform(TransformViewData data) {
        if (!HierarchyPane.Selected.HasValue) {
            return;
        }

        World.Set(
            HierarchyPane.Selected.Value,
            new LocalTransform(data.Position.Value, data.Rotation.Value, data.Scale.Value)
        );
    }

    Vector4State[] LoadLocalToWorld() {
        var rows = new Vector4State[] { new(), new(), new(), new() };

        var localToWorld = World.Get<LocalToWorld>(HierarchyPane.Selected.Value).Value;
        rows[0].SetNext(new(localToWorld.M11, localToWorld.M12, localToWorld.M13, localToWorld.M14));
        rows[1].SetNext(new(localToWorld.M21, localToWorld.M22, localToWorld.M23, localToWorld.M24));
        rows[2].SetNext(new(localToWorld.M31, localToWorld.M32, localToWorld.M33, localToWorld.M34));
        rows[3].SetNext(new(localToWorld.M41, localToWorld.M42, localToWorld.M43, localToWorld.M44));

        return rows;
    }

    protected override void OnRender() {
        if (!HierarchyPane.Selected.HasValue) {
            return;
        }
        
        InspectorHeaderView.InspectorHeaderData headerData = new(SelectedEntity);

        new InspectorHeaderView(headerData, Gui.Project.Tags.ToArray(), Gui.Project.Tags.ToArray()).Render();
        var components = new List<View>();
        List<Action> afterRender = new();

        foreach (var component in World.GetAllComponents(SelectedEntity)) {
            switch (component) {
                case LocalTransform:
                    var data = LoadTransform();
                    components.Add(new TransformView(data));
                    afterRender.Add(() => SaveTransform(data));
                    break;

                case LocalToWorld:
                    components.Add(new LocalToWorldView(LoadLocalToWorld()));
                    break;

                case MeshFilter:
                    components.Add(new MeshFilterView());
                    break;
                
                case ITag:
                    components.Add(new GenericComponentView("tag"));
                    break;

                default:
                    if (!component.GetType().IsGenericType) {
                        components.Add(new GenericComponentView(component.GetType().Name));
                    }

                    break;
            }
        }

        new InspectorView(components.ToArray()).Render();

        foreach (var callback in afterRender) {
            callback();
        }
        
        headerData.Apply();
    }
}
