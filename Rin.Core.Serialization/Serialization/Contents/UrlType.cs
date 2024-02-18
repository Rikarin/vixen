namespace Rin.Core.Serialization.Serialization.Contents;

/// <summary>
///     An enum representing the type of an url.
/// </summary>
[DataContract]
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
