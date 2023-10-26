namespace Rin.Core.IO;

/// <summary>
///     � file event used notified by <see cref="DirectoryWatcher" />
/// </summary>
public class FileEvent : EventArgs {
    /// <summary>
    ///     Gets the type of the change.
    /// </summary>
    /// <value>The type of the change.</value>
    public FileEventChangeType ChangeType { get; }

    /// <summary>
    ///     Gets the name.
    /// </summary>
    /// <value>The name.</value>
    public string Name { get; }

    /// <summary>
    ///     Gets the full path.
    /// </summary>
    /// <value>The full path.</value>
    public string FullPath { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="FileEvent" /> class.
    /// </summary>
    /// <param name="changeType">Type of the change.</param>
    /// <param name="name">The name.</param>
    /// <param name="fullPath">The full path.</param>
    public FileEvent(FileEventChangeType changeType, string name, string fullPath) {
        ChangeType = changeType;
        Name = name;
        FullPath = fullPath;
    }
}