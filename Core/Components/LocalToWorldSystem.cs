using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Rin.Core.Math;
using System.Numerics;

namespace Rin.Core.Components;

public partial class LocalToWorldSystem : BaseSystem<World, float> {
    public LocalToWorldSystem(World world) : base(world) { }

    [Query]
    public void Transform(in Entity entity, ref LocalToWorld localToWorld, ref LocalTransform localTransform) {
        var parent = entity.GetParent();
        var parentTransform = parent.HasValue ? parent.Value.Get<LocalToWorld>().Value : Matrix4x4.Identity;
        
        var scale = localTransform.Scale;
        localToWorld.Value = Matrix.TRS(localTransform.Position, localTransform.Rotation, new(scale, scale, scale)) * parentTransform;

        // Log.Information("Debug: {Variable}", entity.Id);
    }
}
