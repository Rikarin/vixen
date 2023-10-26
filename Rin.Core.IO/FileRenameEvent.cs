namespace Rin.Core.IO;

/// <summary>
///     � file rename event used notified by <see cref="DirectoryWatcher" />
/// </summary>
public class FileRenameEvent : FileEvent {
    /// <summary>
    ///     Gets the full path. (in case of rename)
    /// </summary>
    /// <value>The full path. (in case of rename)</value>
    public string OldFullPath { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="FileRenameEvent" /> class.
    /// </summary>
    /// <param name="changeType">Type of the change.</param>
    /// <param name="name">The name.</param>
    /// <param name="fullPath">The full path.</param>
    /// <param name="oldFullPath">The old full path. (before rename) </param>
    public FileRenameEvent(string name, string fullPath, string oldFullPath) : base(
        FileEventChangeType.Renamed,
        name,
        fullPath
    ) {
        OldFullPath = oldFullPath;
    }

    public override string ToString() => $"{ChangeType}: {FullPath} -> {OldFullPath}";
}
