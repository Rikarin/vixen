using Arch.Core;
using Rin.Core.Components;
using Rin.Core.General;
using Rin.Core.UI;

namespace Rin.Editor.Panes.Inspector;

public class InspectorHeaderView : View {
    readonly string[] tags;
    readonly string[] layers;

    
    public class InspectorHeaderData {
        readonly Entity entity;
        
        public State<bool> IsEnabled { get; set; } = new();
        public State<string> Name { get; set; } = new();
        public State<bool> IsStatic { get; set; } = new();
        
        public State<string> Tag { get; set; } = new();
        public State<string> Layer { get; set; } = new();

        public InspectorHeaderData(Entity entity) {
            this.entity = entity;
            
            var world = SceneManager.ActiveScene!.World;
            IsEnabled.SetNext(world.Has<IsEnabledTag>(entity));

            if (world.TryGet(entity, typeof(Name), out var name)) {
                Name.SetNext(((Name)name).Value);
            }
        }

        public void Apply() {
            var world = SceneManager.ActiveScene!.World;
            
            if (IsEnabled.Value) {
                world.Add<IsEnabledTag>(entity);
            } else {
                world.Remove<IsEnabledTag>(entity);
            }

            if (string.IsNullOrWhiteSpace(Name.Value)) {
                world.Remove<Name>(entity);
            } else {
                world.Add(entity, new Name(Name.Value));
            }
        }
    }

    readonly InspectorHeaderData data;

    public InspectorHeaderView(InspectorHeaderData data, string[] tags, string[] layers) {
        this.data = data;
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
