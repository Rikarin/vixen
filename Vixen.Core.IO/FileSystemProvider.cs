namespace Vixen.Core.IO;

/// <summary>
///     A file system implementation for IVirtualFileProvider.
/// </summary>
public class FileSystemProvider : VirtualFileProviderBase {
    public static readonly char VolumeSeparatorChar = Path.VolumeSeparatorChar;

    public static readonly char DirectorySeparatorChar = Path.DirectorySeparatorChar;

    // TODO: fix this
    public static readonly char AltDirectorySeparatorChar = AltDirectorySeparatorChar == '/' ? '\\' : '/';

    /// <summary>
    ///     Base path of this provider (every path will be relative to this one).
    /// </summary>
    string? localBasePath;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FileSystemProvider" /> class with the given base path.
    /// </summary>
    /// <param name="rootPath">The root path of this provider.</param>
    /// <param name="localBasePath">The path to a local directory where this instance will load the files from.</param>
    public FileSystemProvider(string rootPath, string? localBasePath) : base(rootPath) {
        ChangeBasePath(localBasePath);
    }

    public void ChangeBasePath(string? basePath) {
        localBasePath = basePath;
        localBasePath = localBasePath?.Replace(AltDirectorySeparatorChar, DirectorySeparatorChar);

        // Ensure localBasePath ends with a \
        if (localBasePath != null && !localBasePath.EndsWith(DirectorySeparatorChar)) {
            localBasePath += DirectorySeparatorChar;
        }
    }

    protected virtual string ConvertUrlToFullPath(string url) {
        if (localBasePath == null) {
            return url;
        }

        return localBasePath + url.Replace(VirtualFileSystem.DirectorySeparatorChar, DirectorySeparatorChar);
    }

    protected virtual string ConvertFullPathToUrl(string path) {
        if (localBasePath == null) {
            return path;
        }

        if (!path.StartsWith(localBasePath, StringComparison.OrdinalIgnoreCase)) {
            throw new InvalidOperationException(
                "Trying to convert back a path that is not in this file system provider."
            );
        }

        return path.Substring(localBasePath.Length)
            .Replace(DirectorySeparatorChar, VirtualFileSystem.DirectorySeparatorChar);
    }

    public override bool DirectoryExists(string url) {
        var path = ConvertUrlToFullPath(url);
        return Directory.Exists(path);
    }

    /// <inheritdoc />
    public override void CreateDirectory(string url) {
        var path = ConvertUrlToFullPath(url);
        try {
            Directory.CreateDirectory(path);
        } catch (Exception ex) {
            throw new InvalidOperationException($"Unable to create directory [{path}]", ex);
        }
    }

    /// <inheritdoc />
    public override bool FileExists(string url) => File.Exists(ConvertUrlToFullPath(url));

    public override long FileSize(string url) {
        var fileInfo = new FileInfo(ConvertUrlToFullPath(url));
        return fileInfo.Length;
    }

    /// <inheritdoc />
    public override void FileDelete(string url) {
        File.Delete(ConvertUrlToFullPath(url));
    }

    /// <inheritdoc />
    public override void FileMove(string sourceUrl, string destinationUrl) {
        File.Move(ConvertUrlToFullPath(sourceUrl), ConvertUrlToFullPath(destinationUrl));
    }

    /// <inheritdoc />
    public override void FileMove(string sourceUrl, IVirtualFileProvider destinationProvider, string destinationUrl) {
        if (destinationProvider is FileSystemProvider fsProvider) {
            destinationProvider.CreateDirectory(
                destinationUrl.Substring(0, destinationUrl.LastIndexOf(VirtualFileSystem.DirectorySeparatorChar))
            );
            File.Move(ConvertUrlToFullPath(sourceUrl), fsProvider.ConvertUrlToFullPath(destinationUrl));
        } else {
            using (Stream sourceStream = OpenStream(sourceUrl, VirtualFileMode.Open, VirtualFileAccess.Read),
                   destinationStream = destinationProvider.OpenStream(
                       destinationUrl,
                       VirtualFileMode.CreateNew,
                       VirtualFileAccess.Write
                   )) {
                sourceStream.CopyTo(destinationStream);
            }

            File.Delete(sourceUrl);
        }
    }

    public override string GetAbsolutePath(string path) => ConvertUrlToFullPath(path);

    /// <inheritdoc />
    public override bool TryGetFileLocation(string path, out string filePath, out long start, out long end) {
        filePath = ConvertUrlToFullPath(path);
        start = 0;
        end = -1;
        return true;
    }

    /// <inheritdoc />
    public override string[] ListFiles(string url, string searchPattern, VirtualSearchOption searchOption) =>
        Directory.GetFiles(ConvertUrlToFullPath(url), searchPattern, (SearchOption)searchOption)
            .Select(ConvertFullPathToUrl)
            .ToArray();

#if PLATFORM_IOS
        public bool AutoSetSkipBackupAttribute { get; set; }
#endif

    /// <inheritdoc />
    public override Stream OpenStream(
        string url,
        VirtualFileMode mode,
        VirtualFileAccess access,
        VirtualFileShare share = VirtualFileShare.Read,
        StreamFlags streamType = StreamFlags.None
    ) {
        if (localBasePath != null
            && url.Split(VirtualFileSystem.DirectorySeparatorChar, VirtualFileSystem.AltDirectorySeparatorChar)
                .Contains("..")) {
            throw new InvalidOperationException("Relative path is not allowed in FileSystemProvider.");
        }

        var filename = ConvertUrlToFullPath(url);
        var result = new FileStream(filename, (FileMode)mode, (FileAccess)access, (FileShare)share);

#if PLATFORM_IOS
        if (
            AutoSetSkipBackupAttribute
            && mode is VirtualFileMode.CreateNew or VirtualFileMode.Create or VirtualFileMode.OpenOrCreate
        ) {
            Foundation.NSFileManager.SetSkipBackupAttribute(filename, true);
        }
#endif

        return result;
    }

    public override DateTime GetLastWriteTime(string url) => File.GetLastWriteTime(ConvertUrlToFullPath(url));
}
