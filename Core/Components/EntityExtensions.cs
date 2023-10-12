using Arch.Core;
using Arch.Relationships;

namespace Rin.Core.Components;

public static class EntityExtensions {
    public static Entity? GetParent(this Entity entity) {
        if (entity.HasRelationship<Parent>()) {
            foreach (var parent in entity.GetRelationships<Parent>()) {
                return parent.Key;
            }
        }

        return null;
    }
}
