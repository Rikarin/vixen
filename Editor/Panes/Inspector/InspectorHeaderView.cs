using Arch.Core;
using Rin.Core.Components;
using Rin.Core.General;
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


    public class InspectorHeaderData {
        readonly Entity entity;

        public State<string> EntityId { get; } = new();
        // public State<string> EntityVersion { get; } = new();

        public State<bool> IsEnabled { get; } = new();
        public State<string> Name { get; } = new();
        public State<bool> IsStatic { get; } = new();

        public State<string> Tag { get; } = new();
        public State<string> Layer { get; } = new();

        public InspectorHeaderData(Entity entity) {
            this.entity = entity;
            
            EntityId.SetNext(entity.Id.ToString());

            var world = SceneManager.ActiveScene!.World;
            IsEnabled.SetNext(!world.Has<IsDisabledTag>(entity));

            if (world.TryGet(entity, typeof(Name), out var name)) {
                Name.SetNext(((Name)name).Value);
            }
        }

        public void Apply() {
            var name = Name.Value;

            Application.InvokeOnMainThread(
                () => {
                    var world = SceneManager.ActiveScene!.World;

                    if (IsEnabled.Value) {
                        if (world.Has<IsDisabledTag>(entity)) {
                            world.Remove<IsDisabledTag>(entity);
                        }
                    } else {
                        if (!world.Has<IsDisabledTag>(entity)) {
                            world.Add<IsDisabledTag>(entity);
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(name)) {
                        if (world.Has<Name>(entity)) {
                            world.Set<Name>(entity, new(name));
                        } else {
                            world.Add(entity, new Name(name));
                        }
                    } else if (world.Has<Name>(entity)) {
                        world.Remove<Name>(entity);
                    }
                }
            );
        }
    }
}
