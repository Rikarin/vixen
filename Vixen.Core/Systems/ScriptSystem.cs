using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Arch.System.SourceGenerator;
using Vixen.Core.Components;

namespace Vixen.Core.Systems;

public partial class ScriptSystem : BaseSystem<World, float> {
    public ScriptSystem(World world) : base(world) { }

    [Query]
    [All<IsScriptEnabled>]
    public void Transform(in Entity entity, ref IsScriptEnabled isScriptEnabled) {
        foreach (var component in entity.GetAllComponents()) {
            if (component is IScript script) {
                if (!isScriptEnabled.IsInitialized) {
                    script.Initialize(entity);
                    isScriptEnabled.IsInitialized = true;
                }
                
                script.OnUpdate();
            }
        }
    }
}
