using System.Reflection;

namespace Rin.Core;

/// <summary>
///     Directories used for the running platform.
/// </summary>
public class PlatformDirectories {
    // TODO: This class should not try to initialize directories...etc. Try to find another way to do this

    /// <summary>
    ///     The system temporary directory.
    /// </summary>
    public static readonly string TemporaryDirectory = GetTemporaryDirectory();

    /// <summary>
    ///     The Application temporary directory.
    /// </summary>
    public static readonly string ApplicationTemporaryDirectory = GetApplicationTemporaryDirectory();

    /// <summary>
    ///     The application local directory, where user can write local data (included in backup).
    /// </summary>
    public static readonly string ApplicationLocalDirectory = GetApplicationLocalDirectory();

    /// <summary>
    ///     The application roaming directory, where user can write roaming data (included in backup).
    /// </summary>
    public static readonly string ApplicationRoamingDirectory = GetApplicationRoamingDirectory();

    /// <summary>
    ///     The application cache directory, where user can write data that won't be backup.
    /// </summary>
    public static readonly string ApplicationCacheDirectory = GetApplicationCacheDirectory();

    /// <summary>
    ///     The application data directory, where data is deployed.
    ///     It could be read-only on some platforms.
    /// </summary>
    public static readonly string ApplicationDataDirectory = GetApplicationDataDirectory();

    /// <summary>
    ///     The application directory, where assemblies are deployed.
    ///     It could be read-only on some platforms.
    /// </summary>
    public static readonly string ApplicationBinaryDirectory = GetApplicationBinaryDirectory();

    /// <summary>
    ///     Get the path to the application executable.
    /// </summary>
    /// <remarks>Might be null if start executable is unknown.</remarks>
    public static readonly string ApplicationExecutablePath = GetApplicationExecutablePath();

    static string applicationDataSubDirectory = string.Empty;

    /// <summary>
    ///     The (optional) application data subdirectory. If not null or empty, /data will be mounted on
    ///     <see cref="ApplicationDataDirectory" />/<see cref="ApplicationDataSubDirectory" />
    /// </summary>
    /// <remarks>
    ///     This property should not be written after the VirtualFileSystem static initialization. If so, an
    ///     InvalidOperationException will be thrown.
    /// </remarks>
    public static string ApplicationDataSubDirectory {
        get => applicationDataSubDirectory;

        set {
            if (IsVirtualFileSystemInitialized) {
                throw new InvalidOperationException(
                    "ApplicationDataSubDirectory cannot be modified after the VirtualFileSystem has been initialized."
                );
            }

            applicationDataSubDirectory = value;
        }
    }

    // TODO: internal from Core.IO
    public static bool IsVirtualFileSystemInitialized { get; set; }

    static string GetApplicationLocalDirectory() {
#if PLATFORM_ANDROID
            var directory = Path.Combine(PlatformAndroid.Context.FilesDir.AbsolutePath, "local");
            Directory.CreateDirectory(directory);
            return directory;
#elif PLATFORM_IOS
            return Directory.CreateDirectory(directory);
#else
        // TODO: Should we add "local" ?
        var directory = Path.Combine(GetApplicationBinaryDirectory(), "local");
        Directory.CreateDirectory(directory);
        return directory;
#endif
    }

    static string GetApplicationRoamingDirectory() {
#if PLATFORM_ANDROID
            var directory = Path.Combine(PlatformAndroid.Context.FilesDir.AbsolutePath, "roaming");
            Directory.CreateDirectory(directory);
            return directory;
#elif PLATFORM_IOS
            var directory = Directory.CreateDirectory(directory);
            return directory;
#else
        // TODO: Should we add "local" ?
        var directory = Path.Combine(GetApplicationBinaryDirectory(), "roaming");
        Directory.CreateDirectory(directory);
        return directory;
#endif
    }

    static string GetApplicationCacheDirectory() {
#if PLATFORM_ANDROID
            var directory = Path.Combine(PlatformAndroid.Context.FilesDir.AbsolutePath, "cache");
#elif PLATFORM_IOS
            var directory =
#else
        // TODO: Should we add "local" ?
        var directory = Path.Combine(GetApplicationBinaryDirectory(), "cache");
#endif
        Directory.CreateDirectory(directory);
        return directory;
    }

    static string GetApplicationExecutablePath() => Assembly.GetEntryAssembly()?.Location!;

    static string GetTemporaryDirectory() => GetApplicationTemporaryDirectory();

    static string GetApplicationTemporaryDirectory() {
#if PLATFORM_ANDROID
            return PlatformAndroid.Context.CacheDir.AbsolutePath;
#elif PLATFORM_IOS
            return Path.Combine (Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "..", "tmp");
#else
        return Path.GetTempPath();
#endif
    }

    static string GetApplicationBinaryDirectory() {
#if PLATFORM_ANDROID
            return GetApplicationExecutableDirectory();
#else
        return Path.GetDirectoryName(typeof(PlatformDirectories).Assembly.Location)!;
#endif
    }

    static string GetApplicationExecutableDirectory() => AppContext.BaseDirectory;

    static string GetApplicationDataDirectory() {
#if PLATFORM_ANDROID
            return Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/Android/data/" + PlatformAndroid.Context.PackageName + "/data";
#elif PLATFORM_IOS
            return Foundation.NSBundle.MainBundle.BundlePath + "/data";
#else
        return Path.Combine(GetApplicationBinaryDirectory(), "data");
#endif
    }
}
