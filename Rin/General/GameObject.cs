namespace Editor.General;

public class GameObject : BaseObject {
    readonly List<Component> components = new() { new Transform() };

    // TODO: consider using code generator to generate enum based on values specified in the editor
    public int Layer { get; set; }

    public string Tag { get; set; }

    public bool IsActive { get; private set; }

    public bool IsActiveInHierarchy => throw new NotImplementedException();

    public Transform Transform => (Transform)components[0];


    public void SetActive(bool active) {
        IsActive = active;
    }

    public Component AddComponent(Type component) => throw new NotImplementedException();

    public T AddComponent<T>() where T : Component => (T)AddComponent(typeof(T));

    public bool CompareTag(string tag) => string.Equals(Tag, tag, StringComparison.InvariantCultureIgnoreCase);
}
