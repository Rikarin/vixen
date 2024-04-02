using Arch.Core;
using Vixen.Core.General;
using Vixen.InputSystem;

namespace Vixen.Core.Components;

public record struct IsScriptEnabled(bool IsInitialized) : ITag;


// TODO: Not sure about the Entity and Initialize methods
public interface IScript {
    Entity Entity { get; }
    
    void OnStart();
    void OnUpdate();
    void Initialize(Entity entity);
}

public abstract class Script : IScript {
    public InputManager Input => InputContainer.inputManager;
    public Entity Entity { get; private set; }
    
    public virtual void OnStart() { }
    public virtual void OnUpdate() { }

    public void Initialize(Entity entity) {
        Entity = entity;
        OnStart();
    }
}
