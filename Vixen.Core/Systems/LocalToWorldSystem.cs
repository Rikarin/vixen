using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using System.Numerics;
using Vixen.Core.Common.Mathematics;
using Vixen.Core.Components;

namespace Vixen.Core.Systems;

public partial class LocalToWorldSystem : BaseSystem<World, float> {
    public LocalToWorldSystem(World world) : base(world) { }

    [Query]
    public void Transform(in Entity entity, ref LocalToWorld localToWorld, in LocalTransform localTransform) {
        var parent = entity.GetParent();

        if (!parent.HasValue || !parent.Value.TryGet<LocalToWorld>(out var parentTransform)) {
            parentTransform = new(Matrix4x4.Identity);
        }

        var scale = localTransform.Scale;
        localToWorld.Value = parentTransform.Value
            * Matrix.TRS(localTransform.Position, localTransform.Rotation, new(scale, scale, scale));
    }
}
