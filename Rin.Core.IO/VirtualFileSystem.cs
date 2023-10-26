using System.Text;
using System.Text.RegularExpressions;

namespace Rin.Core.IO;

/// <summary>
///     Virtual abstraction over a file system.
///     It handles access to files, http, packages, path rewrite, etc...
/// </summary>
public static class VirtualFileSystem {
    public static readonly char DirectorySeparatorChar = '/';
    public static readonly char AltDirectorySeparatorChar = '\\';
    public static readonly char[] AllDirectorySeparatorChars = { DirectorySeparatorChar, AltDirectorySeparatorChar };
    public static readonly string ApplicationDatabasePath = "/data/db";
    public static readonly string LocalDatabasePath = "/local/db";
    public static readonly string ApplicationDatabaseIndexName = "index";

    public static readonly string ApplicationDatabaseIndexPath =
        ApplicationDatabasePath + DirectorySeparatorChar + ApplicationDatabaseIndexName;

    /// <summary>
    ///     The application data file provider.
    /// </summary>
    public static readonly IVirtualFileProvider ApplicationData;

    /// <summary>
    ///     The application database file provider (ObjectId level).
    /// </summary>
    public static IVirtualFileProvider ApplicationObjectDatabase;

    /// <summary>
    ///     The application database file provider (Index level).
    /// </summary>
    public static readonly IVirtualFileProvider ApplicationDatabase;

    /// <summary>
    ///     The application cache directory.
    /// </summary>
    public static readonly IVirtualFileProvider ApplicationCache;

    /// <summary>
    ///     The application user roaming directory. Included in backup.
    /// </summary>
    public static readonly IVirtualFileProvider ApplicationRoaming;

    /// <summary>
    ///     The application user local directory. Included in backup.
    /// </summary>
    public static readonly IVirtualFileProvider ApplicationLocal;

    /// <summary>
    ///     The application temporary data provider.
    /// </summary>
    public static readonly IVirtualFileProvider ApplicationTemporary;

    /// <summary>
    ///     The application binary directory.
    /// </summary>
    public static readonly IVirtualFileProvider ApplicationBinary;

    /// <summary>
    ///     The whole host file system. This should be used only in tools.
    /// </summary>
    public static readonly DriveFileProvider Drive;

    static readonly Regex PathSplitRegex = new(@"(\\|/)");

    // As opposed to real Path.GetTempFileName, we don't have a 65536 limit.
    // This can be achieved by having a fixed random seed.
    // However, if activated, it would probably test too many files in the same order, if some already exists.
    static readonly Random tempFileRandom = new(Environment.TickCount);

    static readonly Dictionary<string, IVirtualFileProvider> providers = new();

    /// <summary>
    ///     Gets the registered providers.
    /// </summary>
    /// <value>The providers.</value>
    public static IEnumerable<IVirtualFileProvider> Providers => providers.Values;

    /// <summary>
    ///     Initializes static members of the <see cref="VirtualFileSystem" /> class.
    /// </summary>
    static VirtualFileSystem() {
        PlatformDirectories.IsVirtualFileSystemInitialized = true;
        
        // TODO: find a better solution to customize the ApplicationDataDirectory, now we're very limited due to the initialization from a static constructor
#if PLATFORM_ANDROID
            ApplicationData = new ZipFileSystemProvider("/data", PlatformAndroid.Context.ApplicationInfo.SourceDir);
#else
        ApplicationData = new FileSystemProvider(
            "/data",
            Path.Combine(PlatformDirectories.ApplicationDataDirectory, PlatformDirectories.ApplicationDataSubDirectory)
        );
#endif
        ApplicationCache = new FileSystemProvider("/cache", PlatformDirectories.ApplicationCacheDirectory);
#if PLATFORM_IOS
            // On iOS, we don't want cache directory to be cleared by the OS.
            ((FileSystemProvider)ApplicationCache).AutoSetSkipBackupAttribute = true;
#endif
        ApplicationRoaming = new FileSystemProvider("/roaming", PlatformDirectories.ApplicationRoamingDirectory);
        ApplicationLocal = new FileSystemProvider("/local", PlatformDirectories.ApplicationLocalDirectory);
        ApplicationTemporary = new FileSystemProvider("/tmp", PlatformDirectories.ApplicationTemporaryDirectory);
        ApplicationBinary = new FileSystemProvider("/binary", PlatformDirectories.ApplicationBinaryDirectory);
        Drive = new(DriveFileProvider.DefaultRootPath);
    }

    /// <summary>
    ///     Registers the specified virtual file provider at the specified mount location.
    /// </summary>
    /// <param name="provider">The provider.</param>
    public static void RegisterProvider(IVirtualFileProvider provider) {
        if (provider.RootPath != null) {
            if (providers.ContainsKey(provider.RootPath)) {
                throw new InvalidOperationException(
                    $"A Virtual File Provider with the root path \"{provider.RootPath}\" already exists."
                );
            }

            providers.Add(provider.RootPath, provider);
        }
    }

    /// <summary>
    ///     Unregisters the specified virtual file provider.
    /// </summary>
    /// <param name="provider">The provider.</param>
    /// <param name="dispose">Indicate that the provider should be disposed, if it inherits from IDisposable interface.</param>
    public static void UnregisterProvider(IVirtualFileProvider provider, bool dispose = true) {
        var mountPoints = providers.Where(x => x.Value == provider).ToArray();
        foreach (var mountPoint in mountPoints) {
            providers.Remove(mountPoint.Key);
        }
    }

    /// <summary>
    ///     Mounts the specified path in the specified virtual file mount point.
    /// </summary>
    /// <param name="mountPoint">The mount point in the VFS.</param>
    /// <param name="path">The directory path.</param>
    public static IVirtualFileProvider MountFileSystem(string mountPoint, string path) =>
        new FileSystemProvider(mountPoint, path);

    /// <summary>
    ///     Mounts or remounts the specified path in the specified virtual file mount point.
    /// </summary>
    /// <param name="mountPoint">The mount point in the VFS.</param>
    /// <param name="path">The directory path.</param>
    public static IVirtualFileProvider RemountFileSystem(string mountPoint, string path) {
        // Ensure mount point is terminated with a /
        if (mountPoint[^1] != DirectorySeparatorChar) {
            mountPoint += DirectorySeparatorChar;
        }

        // Find existing provider
        var provider = providers.FirstOrDefault(x => x.Key == mountPoint);
        if (provider.Value != null) {
            ((FileSystemProvider)provider.Value).ChangeBasePath(path);
            return provider.Value;
        }

        // Otherwise create new one
        return new FileSystemProvider(mountPoint, path);
    }

    /// <summary>
    ///     Checks the existence of a file.
    /// </summary>
    /// <param name="path">The path of the file to check.</param>
    /// <returns>True if the file exists, false otherwise.</returns>
    public static bool FileExists(string path) {
        if (path == null) {
            throw new ArgumentNullException(nameof(path));
        }

        var result = ResolveProviderUnsafe(path, true);
        return result.Provider != null && result.Provider.FileExists(result.Path);
    }

    /// <summary>
    ///     Checks the existence of a directory.
    /// </summary>
    /// <param name="path">The path of the directory to check.</param>
    /// <returns>True if the directory exists, false otherwise.</returns>
    public static bool DirectoryExists(string path) {
        if (path == null) {
            throw new ArgumentNullException(nameof(path));
        }

        var result = ResolveProviderUnsafe(path, true);
        if (result.Provider == null) {
            return false;
        }

        return result.Provider.DirectoryExists(result.Path);
    }

    public static void FileDelete(string path) {
        var result = ResolveProvider(path, true);
        result.Provider.FileDelete(result.Path);
    }

    public static void FileMove(string sourcePath, string destinationPath) {
        var sourceResult = ResolveProvider(sourcePath, true);
        var destinationResult = ResolveProvider(destinationPath, true);

        if (sourceResult.Provider == destinationResult.Provider) {
            sourceResult.Provider.FileMove(sourceResult.Path, destinationResult.Path);
        } else {
            sourceResult.Provider.FileMove(sourceResult.Path, destinationResult.Provider, destinationResult.Path);
        }
    }

    public static long FileSize(string path) {
        var result = ResolveProvider(path, true);
        return result.Provider.FileSize(result.Path);
    }

    public static DateTime GetLastWriteTime(string path) {
        var result = ResolveProvider(path, true);
        return result.Provider.GetLastWriteTime(result.Path);
    }

    public static Task<bool> FileExistsAsync(string path) {
        return Task<bool>.Factory.StartNew(() => FileExists(path));
    }

    /// <summary>
    ///     Creates all directories so that path exists.
    /// </summary>
    /// <param name="path">The path.</param>
    public static void CreateDirectory(string path) {
        var resolveProviderResult = ResolveProvider(path, true);
        resolveProviderResult.Provider.CreateDirectory(resolveProviderResult.Path);
    }

    /// <summary>
    ///     Opens the stream from a given path.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="mode">The stream opening mode (append, open, create, etc...).</param>
    /// <param name="access">The stream access.</param>
    /// <param name="share">The stream share mode.</param>
    /// <returns>The stream.</returns>
    public static Stream OpenStream(
        string path,
        VirtualFileMode mode,
        VirtualFileAccess access,
        VirtualFileShare share = VirtualFileShare.Read
    ) {
        var resolveProviderResult = ResolveProvider(path, false);
        return resolveProviderResult.Provider.OpenStream(resolveProviderResult.Path, mode, access, share);
    }

    /// <summary>
    ///     Opens the stream from a given path.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="mode">The stream opening mode (append, open, create, etc...).</param>
    /// <param name="access">The stream access.</param>
    /// <param name="share">The stream share mode.</param>
    /// <param name="provider">The provider used to load the stream.</param>
    /// <returns>The stream.</returns>
    public static Stream OpenStream(
        string path,
        VirtualFileMode mode,
        VirtualFileAccess access,
        VirtualFileShare share,
        out IVirtualFileProvider provider
    ) {
        var resolveProviderResult = ResolveProvider(path, false);
        provider = resolveProviderResult.Provider;
        return provider.OpenStream(resolveProviderResult.Path, mode, access, share);
    }

    public static Task<Stream> OpenStreamAsync(
        string path,
        VirtualFileMode mode,
        VirtualFileAccess access,
        VirtualFileShare share = VirtualFileShare.Read
    ) {
        return Task<Stream>.Factory.StartNew(() => OpenStream(path, mode, access, share));
    }

    /// <summary>
    ///     Gets the absolute path (system dependent) for the specified path in the context of the virtual file system.
    /// </summary>
    /// <param name="path">The path local to the virtual file system.</param>
    /// <returns>An absolute path (system dependent .i.e C:\Path\To\Your\File.x).</returns>
    public static string GetAbsolutePath(string path) {
        var resolveProviderResult = ResolveProvider(path, true);
        return resolveProviderResult.Provider.GetAbsolutePath(resolveProviderResult.Path);
    }

    /// <summary>
    ///     Resolves the path.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>The resolved path.</returns>
    public static string ResolvePath(string path) {
        var resolveProviderResult = ResolveProvider(path, false);

        var sb = new StringBuilder();
        if (resolveProviderResult.Provider.RootPath != ".") {
            sb.Append(resolveProviderResult.Provider.RootPath);
            sb.Append("/");
        }

        sb.Append(resolveProviderResult.Path);

        return sb.ToString();
    }

    /// <summary>
    ///     Lists the files matching a pattern in a specified directory.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="searchPattern">The search pattern.</param>
    /// <param name="searchOption">The search option.</param>
    /// <returns>The list of files matching the pattern.</returns>
    public static Task<string[]> ListFiles(string path, string searchPattern, VirtualSearchOption searchOption) {
        var resolveProviderResult = ResolveProvider(path, true);
        return Task.Factory.StartNew(
            () => resolveProviderResult.Provider.ListFiles(resolveProviderResult.Path, searchPattern, searchOption)
                .Select(x => resolveProviderResult.Provider.RootPath + x)
                .ToArray()
        );
    }

    /// <summary>
    ///     Creates a temporary zero-byte file and returns its full path.
    /// </summary>
    /// <returns>The full path of the created temporary file.</returns>
    public static string GetTempFileName() {
        var tentatives = 0;
        Stream? stream = null;
        string filename;
        do {
            filename = "sd" + (tempFileRandom.Next() + 1).ToString("x") + ".tmp";
            try {
                stream = ApplicationTemporary.OpenStream(
                    filename,
                    VirtualFileMode.CreateNew,
                    VirtualFileAccess.ReadWrite
                );
            } catch (IOException) {
                // No more than 65536 files
                if (tentatives++ > 0x10000) {
                    throw;
                }
            }
        } while (stream == null);

        stream.Dispose();
        return ApplicationTemporary.RootPath + "/" + filename;
    }

    public static string BuildPath(string path, string relativePath) =>
        path[..(LastIndexOfDirectorySeparator(path) + 1)] + relativePath;

    /// <summary>
    ///     Returns the path with its .. or . parts simplified.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>The resolved absolute path.</returns>
    public static string ResolveAbsolutePath(string path) {
        if (!path.Contains(DirectorySeparatorChar + "..")) {
            return path;
        }

        var pathElements = PathSplitRegex.Split(path).ToList();

        // Remove duplicate directory separators
        for (var i = 0; i < pathElements.Count; ++i) {
            if (pathElements[i].Length > 1 && (pathElements[i][0] == '/' || pathElements[i][0] == '\\')) {
                pathElements[i] = pathElements[i][0].ToString();
            }
        }

        for (var i = 0; i < pathElements.Count; ++i) {
            if (pathElements[i] == "..") {
                // Remove .. and the item preceding that, if any
                if (i >= 3 && (pathElements[i - 1] == "/" || pathElements[i - 1] == "\\")) {
                    pathElements.RemoveRange(i - 3, 4);
                    i -= 4;
                }
            } else if (pathElements[i] == ".") {
                if (i >= 1 && (pathElements[i - 1] == "/" || pathElements[i - 1] == "\\")) {
                    pathElements.RemoveRange(i - 1, 2);
                    i -= 2;
                } else if (i + 1 < pathElements.Count && (pathElements[i + 1] == "/" || pathElements[i + 1] == "\\")) {
                    pathElements.RemoveRange(i, 2);
                    --i;
                }
            }
        }

        return string.Join(string.Empty, pathElements);
    }

    /// <summary>
    ///     Combines the specified paths.
    ///     Similar to <see cref="System.IO.Path.Combine(string, string)" />.
    /// </summary>
    /// <param name="path1">The path1.</param>
    /// <param name="path2">The path2.</param>
    /// <returns>The combined path.</returns>
    public static string Combine(string path1, string path2) {
        if (path1.Length == 0) {
            return path2;
        }

        if (path2.Length == 0) {
            return path1;
        }

        var lastPath1 = path1[^1];
        if (lastPath1 != DirectorySeparatorChar && lastPath1 != AltDirectorySeparatorChar) {
            return path1 + DirectorySeparatorChar + path2;
        }

        return path1 + path2;
    }

    /// <summary>
    ///     Gets the parent directory.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>The parent directory.</returns>
    /// <exception cref="System.ArgumentNullException">path</exception>
    /// <exception cref="System.ArgumentException">path doesn't contain a /;path</exception>
    public static string GetParentDirectory(string path) {
        if (path == null) {
            throw new ArgumentNullException(nameof(path));
        }

        var lastSlashIndex = LastIndexOfDirectorySeparator(path);
        if (lastSlashIndex == -1) {
            throw new ArgumentException($"path [{path}] doesn't contain a /", nameof(path));
        }

        return path[..lastSlashIndex];
    }

    /// <summary>
    ///     Gets the file's name with its extension ("/path/to/file/fileName.ext"->"fileName.ext")
    /// </summary>
    /// <param name="path">path containing file's path and name </param>
    /// <returns>The name of the file with its extension</returns>
    public static string GetFileName(string path) {
        if (path == null) {
            throw new ArgumentNullException(nameof(path));
        }

        var lastSlashIndex = LastIndexOfDirectorySeparator(path);
        return path[(lastSlashIndex + 1)..];
    }

    /// <summary>
    ///     Creates the relative path that can access to <paramref name="target" /> from <paramref name="sourcePath" />.
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="sourcePath">The source path.</param>
    /// <returns>The relative path.</returns>
    public static string CreateRelativePath(string target, string sourcePath) {
        var targetDirectories = target.Split(AllDirectorySeparatorChars, StringSplitOptions.RemoveEmptyEntries);
        var sourceDirectories = sourcePath.Split(AllDirectorySeparatorChars, StringSplitOptions.RemoveEmptyEntries);

        // Find common root
        int length = System.Math.Min(targetDirectories.Length, sourceDirectories.Length);
        int commonRoot;
        for (commonRoot = 0; commonRoot < length; ++commonRoot) {
            if (targetDirectories[commonRoot] != sourceDirectories[commonRoot]) {
                break;
            }
        }

        var result = new StringBuilder();

        // Append .. for each path only in source
        for (var i = commonRoot; i < sourceDirectories.Length; ++i) {
            result.Append(".." + DirectorySeparatorChar);
        }

        // Append path in destination
        for (var i = commonRoot; i < targetDirectories.Length; ++i) {
            result.Append(targetDirectories[i]);
            if (i < targetDirectories.Length - 1) {
                result.Append(DirectorySeparatorChar);
            }
        }

        return result.ToString();
    }

    /// <summary>
    ///     Resolves the virtual file provider for a given path.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="resolveTop">if set to <c>true</c> [resolve top].</param>
    /// <returns>The virtual file system provider and local path in it.</returns>
    /// <exception cref="System.InvalidOperationException">path cannot be resolved to a provider.</exception>
    public static ResolveProviderResult ResolveProvider(string path, bool resolveTop) {
        if (path == null) {
            throw new ArgumentNullException(nameof(path));
        }

        var result = ResolveProviderUnsafe(path, resolveTop);
        if (result.Provider == null) {
            throw new InvalidOperationException($"path [{path}] cannot be resolved to a provider.");
        }

        return result;
    }

    public static ResolveProviderResult ResolveProviderUnsafe(string path, bool resolveTop) {
        // Slow path for path using \ instead of /
        if (path.Contains(AltDirectorySeparatorChar)) {
            path = path.Replace(AltDirectorySeparatorChar, DirectorySeparatorChar);
        }

        // Resolve using providers at every level of the path (deep first)
        // i.e. provider for path /a/b/c/file will be searched in the following order: /a/b/c/ then /a/b/ then /a/.
        for (var i = path.Length - 1; i >= 0; --i) {
            var pathChar = path[i];
            var isResolvingTop = i == path.Length - 1 && resolveTop;
            if (!isResolvingTop && pathChar != DirectorySeparatorChar) {
                continue;
            }

            var providerPath = isResolvingTop && pathChar != DirectorySeparatorChar
                ?
                new StringBuilder(path.Length + 1).Append(path).Append(DirectorySeparatorChar).ToString()
                : i + 1 == path.Length
                    ? path
                    : path.Substring(0, i + 1);

            if (providers.TryGetValue(providerPath, out var provider)) {
                // If resolving top, we want the / at the end of "path" if it wasn't there already (should be in providerPath).
                if (isResolvingTop) {
                    path = providerPath;
                }

                return new() { Provider = provider, Path = path[providerPath.Length..] };
            }
        }

        return new();
    }

    static int LastIndexOfDirectorySeparator(string path) {
        var length = path.Length;
        while (--length >= 0) {
            var c = path[length];
            if (c == DirectorySeparatorChar || c == AltDirectorySeparatorChar) {
                return length;
            }
        }

        return -1;
    }

    public struct ResolveProviderResult {
        public IVirtualFileProvider? Provider;
        public string Path;
    }
}
