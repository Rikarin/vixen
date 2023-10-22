namespace Rin.Core.TODO;

public readonly record struct ObjectUrl(UrlType Type, string Path) {
    public static readonly ObjectUrl Empty = new();
    public override string ToString() => Path;
}

/// <summary>
///     An enum representing the type of an url.
/// </summary>
// [DataContract]
public enum UrlType {
    /// <summary>
    ///     The location is not valid.
    /// </summary>
    None,

    /// <summary>
    ///     The location is a file on the disk.
    /// </summary>
    File,

    /// <summary>
    ///     The location is a content url.
    /// </summary>
    Content
}
