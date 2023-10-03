namespace Rin.Core.General;

public class GameObject : BaseObject {
    protected readonly Dictionary<Type, List<Component>> components = new();

    // TODO: consider using code generator to generate enum based on values specified in the editor
    public int Layer { get; set; }
    public string Tag { get; set; }
    public bool IsActive { get; private set; }
    public bool IsActiveInHierarchy => throw new NotImplementedException();

    public Transform Transform { get; } = new();
    // readonly List<Component> components = new() { new Transform() };

    internal Guid Id { get; private set; }

    public void SetActive(bool active) {
        IsActive = active;
    }

    public T AddComponent<T>() where T : Component => (T)AddComponent(typeof(T));

    public virtual Component AddComponent(Type component) {
        if (!component.IsAssignableTo(typeof(Component))) {
            throw new("TODO");
        }

        var instance = (Component)Activator.CreateInstance(component)!;
        foreach (var type in component.GetParents()) {
            if (!components.ContainsKey(type)) {
                components.Add(type, new());
            }

            components[type].Add(instance);
        }

        return instance;
    }

    public T GetComponent<T>() where T : Component {
        if (components.TryGetValue(typeof(T), out var value)) {
            return (T)value.First();
        }

        throw new($"Component {typeof(T).Name} not found");
    }

    // public virtual void RemoveComponent(Component component) {
    //     foreach (var type in EntityExtensions.GetParents(component.GetType())) {
    //         components[type].Remove(component);
    //     }
    // }

    public bool CompareTag(string tag) => string.Equals(Tag, tag, StringComparison.InvariantCultureIgnoreCase);
}

static class TypeExtensions {
    public static IEnumerable<Type> GetParents(this Type type) {
        yield return type;

        var currentBaseType = type.BaseType;
        while (currentBaseType != null && currentBaseType != typeof(object)) {
            yield return currentBaseType;
            currentBaseType = currentBaseType.BaseType;
        }
    }
}
