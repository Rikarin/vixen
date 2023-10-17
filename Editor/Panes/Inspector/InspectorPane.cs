using Arch.Core;
using Rin.Core.Components;
using Rin.Core.General;
using Rin.Editor.States;
using Rin.UI;

namespace Rin.Editor.Panes.Inspector;

sealed class InspectorPane : Pane {
    readonly TransformViewData data = new();
    World World => SceneManager.ActiveScene!.World;
    Entity SelectedEntity => HierarchyPane.Selected!.Value;

    public InspectorPane() : base("Inspector") { }

    void LoadTransform() {
        var localTransform = World.Get<LocalTransform>(HierarchyPane.Selected.Value);
        data.Position.SetNext(localTransform.Position);
        data.Rotation.SetNext(localTransform.Rotation);
        // data.Rotation.SetNext(localTransform.Rotation.ToEulerAngles().ToDegrees());
        data.Scale.SetNext(localTransform.Scale);
        
        data.ResetDirty();
    }

    void SaveTransform(TransformViewData data) {
        if (!HierarchyPane.Selected.HasValue || !data.IsDirty) {
            return;
        }
        
        Application.InvokeOnMainThread(
            () => {
                World.Set(
                    HierarchyPane.Selected.Value,
                    // new LocalTransform(data.Position.Value, data.Rotation.Value.ToRadians().ToQuaternion(), data.Scale.Value)
                    new LocalTransform(data.Position.Value, data.Rotation.Value, data.Scale.Value)
                );
            }
        );

        data.ResetDirty();
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

        InspectorHeaderData headerData = new(SelectedEntity);
        new InspectorHeaderView(headerData, Gui.Project.Tags.ToArray(), Gui.Project.Tags.ToArray()).Render();
        var components = new List<View>();
        List<Action> afterRender = new();

        foreach (var component in World.GetAllComponents(SelectedEntity)) {
            if (component == null) {
                continue;
            }

            switch (component) {
                case LocalTransform:
                    LoadTransform();
                    components.Add(new TransformView(data));
                    afterRender.Add(() => SaveTransform(data));
                    break;

                case LocalToWorld:
                    components.Add(new LocalToWorldView(LoadLocalToWorld()));
                    break;

                case MeshFilter:
                    components.Add(new MeshFilterView());
                    break;

                case IsDisabledTag: break;
                case Name: break;

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
