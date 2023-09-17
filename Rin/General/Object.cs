namespace Editor.General;

public abstract class BaseObject {
    // TODO: should be nullable?
    public string? Name { get; set; }

    /// <summary>
    ///     Unique id of object created at runtime
    /// </summary>
    public int InstanceId { get; } = 42;

    /// <summary>
    ///     Create an instance of provided object at runtime
    /// </summary>
    /// <param name="obj"></param>
    /// <returns>new instance of obj</returns>
    /// <exception cref="NotImplementedException"></exception>
    public static BaseObject Instantiate(BaseObject obj) => throw new NotImplementedException();

    /// <summary>
    ///     Destroy object after specified time
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="delay">delay after the object is destroyed</param>
    /// <exception cref="NotImplementedException"></exception>
    public static void Destroy(BaseObject obj, float delay = 0) {
        throw new NotImplementedException();
    }

    public override string ToString() => Name ?? "<unknown>";
}
