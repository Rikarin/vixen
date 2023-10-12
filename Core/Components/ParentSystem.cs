using Arch.Core;
using Arch.Relationships;
using Arch.System;
using Arch.System.SourceGenerator;
using Serilog;

namespace Rin.Core.Components;

public partial class ParentSystem : BaseSystem<World, float> {
    public ParentSystem(World world) : base(world) { }
    
    [Query]
    [All<Relationship<Child>>]
    public void RemoveChildrenFromParent(in Entity parent, in Relationship<Child> children) {
        foreach (var (child, _) in children) {
            if (!child.HasRelationship<Parent>(parent)) {
                parent.RemoveRelationship<Child>(child);
            }
        }
    }

    [Query]
    [All<Relationship<Parent>>]
    public void AddChildrenToParent(in Entity child) {
        var parent = child.GetParent();
        if (parent.HasValue) {
            if (!parent.Value.HasRelationship<Child>(child)) {
                parent.Value.AddRelationship<Child>(child);
                
                Log.Information("Adding Child P {P} C {C}", parent.Value.Id, child.Id);
            }
        } else {
            Log.Error("Doesn't have parent");
        }
    }
}
