namespace Editor.General;

public class GameObject : BaseObject {
    // TODO: consider using code generator to generate enum based on values specified in the editor
    public int Layer { get; set; }

    public string Tag { get; set; }
}
