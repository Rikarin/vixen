using Arch.Core;
using Arch.Relationships;

namespace Vixen.Core.Components;

public static class EntityExtensions {
    public static Entity? GetParent(this Entity entity) {
        if (entity.HasRelationship<Parent>()) {
            Entity? ret = null;
            foreach (var parent in entity.GetRelationships<Parent>()) {
                if (!ret.HasValue) {
                    ret = parent.Key;
                } else {
                    throw new("Multiple parents");
                }
            }

            return ret;
        }

        return null;
    }
}
