using Arch.Core;
using Rin.Core.Components;
using Rin.Core.General;
using Rin.Core.UI;
using Serilog;

namespace Rin.Editor.Panes.Inspector;

public class InspectorHeaderData {
    readonly Entity entity;

    public State<string> EntityId { get; } = new();
    // public State<string> EntityVersion { get; } = new();

    public State<bool> IsEnabled { get; } = new();
    public State<string> Name { get; } = new();
    public State<bool> IsStatic { get; } = new();

    public State<string> Tag { get; } = new();
    public State<string> Layer { get; } = new();

    public bool IsDirty { get; set; }

    public InspectorHeaderData(Entity entity) {
        this.entity = entity;
            
        EntityId.SetNext(entity.Id.ToString());

        var world = SceneManager.ActiveScene!.World;
        IsEnabled.SetNext(!world.Has<IsDisabledTag>(entity));

        if (world.TryGet(entity, typeof(Name), out var name)) {
            Name.SetNext(((Name)name).Value);
        }
        
        IsEnabled.Subscribe(_ => IsDirty = true);
        Name.Subscribe(_ => IsDirty = true);
        // TODO: others if needed

        IsDirty = false;
    }

    public void Apply() {
        if (!IsDirty) {
            return;
        }

        IsDirty = false;
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
