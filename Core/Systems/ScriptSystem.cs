using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Arch.System.SourceGenerator;
using Rin.Core.Components;

namespace Rin.Core.Systems;

public partial class ScriptSystem : BaseSystem<World, float> {
    public ScriptSystem(World world) : base(world) { }

    [Query]
    [All<IsScriptEnabledTag>]
    public void Transform(in Entity entity) {
        foreach (var component in entity.GetAllComponents()) {
            if (component is Script script) {
                script.Initialize(entity);
                script.OnUpdate();
            }
        }
    }
}
