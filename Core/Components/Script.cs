using Arch.Core;

namespace Rin.Core.Components;

public record struct IsScriptEnabledTag : ITag;

public abstract class Script {
    bool initialized;
    protected Entity Entity { get; private set; }
    
    public virtual void OnStart() { }
    public virtual void OnUpdate() { }

    internal void Initialize(Entity entity) {
        if (!initialized) {
            Entity = entity;
            OnStart();
        }
    }
}
