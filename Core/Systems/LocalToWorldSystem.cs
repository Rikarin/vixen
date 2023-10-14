using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Rin.Core.Components;
using Rin.Core.Math;
using System.Numerics;

namespace Rin.Core.Systems;

public partial class LocalToWorldSystem : BaseSystem<World, float> {
    public LocalToWorldSystem(World world) : base(world) { }

    [Query]
    public void Transform(in Entity entity, ref LocalToWorld localToWorld, in LocalTransform localTransform) {
        var parent = entity.GetParent();

        if (!parent.HasValue || !parent.Value.TryGet<LocalToWorld>(out var parentTransform)) {
            parentTransform = new(Matrix4x4.Identity);
        }

        var scale = localTransform.Scale;
        localToWorld.Value = Matrix.TRS(localTransform.Position, localTransform.Rotation, new(scale, scale, scale))
            * parentTransform.Value;
    }
}
