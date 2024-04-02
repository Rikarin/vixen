using Arch.Core;
using Arch.Relationships;
using Arch.System;
using Arch.System.SourceGenerator;
using Vixen.Core.Components;

namespace Vixen.Core.Systems;

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
        var parent = child.GetParent()!.Value;
        
        if (!parent.HasRelationship<Child>(child)) {
            parent.AddRelationship<Child>(child);
        }
    }
}
