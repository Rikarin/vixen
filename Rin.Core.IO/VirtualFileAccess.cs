namespace Rin.Core.IO;

/// <summary>
///     File access equivalent of <see cref="System.IO.FileAccess" />.
/// </summary>
[Flags]
public enum VirtualFileAccess : uint {
    /// <summary>
    ///     Read access.
    /// </summary>
    Read = 1,

    /// <summary>
    ///     Write access.
    /// </summary>
    Write = 2,

    /// <summary>
    ///     Read/Write Access,
    /// </summary>
    ReadWrite = Read | Write
}