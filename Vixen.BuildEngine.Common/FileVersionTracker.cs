using Serilog;
using Vixen.Core.Serialization.Storage;
using Vixen.Core.Storage;

namespace Vixen.BuildEngine.Common;

/// <summary>
///     A tracker of file date.
/// </summary>
public class FileVersionTracker : IDisposable {
    const string DefaultFileVersionTrackerFile = "Vixen/FileVersionTracker.cache";

    readonly FileVersionStorage storage;
    readonly Dictionary<FileVersionKey, object> locks;

    static readonly ILogger Log = Serilog.Log.ForContext<FileVersionTracker>();
    static readonly object LockDefaultTracker = new();
    static FileVersionTracker? defaultFileVersionTracker;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FileVersionTracker" /> class.
    /// </summary>
    /// <param name="stream">The stream.</param>
    public FileVersionTracker(Stream stream) {
        storage = new(stream);
        locks = new();
    }

    /// <summary>
    ///     Gets the default file version tracker for this machine.
    /// </summary>
    /// <returns>FileVersionTracker.</returns>
    public static FileVersionTracker GetDefault() {
        lock (LockDefaultTracker) {
            if (defaultFileVersionTracker == null) {
                var filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    DefaultFileVersionTrackerFile
                );
                var directory = Path.GetDirectoryName(filePath);
                if (directory != null && !Directory.Exists(directory)) {
                    Directory.CreateDirectory(directory);
                }

                // Loads the file version cache
                defaultFileVersionTracker = Load(filePath);
            }
        }

        return defaultFileVersionTracker;
    }

    /// <summary>
    ///     Loads previous versions stored from the specified file path.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns>FileVersionTracker.</returns>
    public static FileVersionTracker Load(string filePath) {
        // Try to compact it before using it
        FileVersionStorage.Compact(filePath);

        var isFirstPass = true;
        while (true) {
            FileStream fileStream = null!;

            // Try to open the file, if we get an exception, this might be due only because someone is locking the file to
            // save it while we are trying to open it
            const int RetryOpenFileStream = 20;
            var random = new Random();
            for (var i = 0; i < RetryOpenFileStream; i++) {
                try {
                    fileStream = new(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                    break;
                } catch (Exception) {
                    if (i + 1 == RetryOpenFileStream) {
                        throw;
                    }

                    Thread.Sleep(50 + random.Next(100));
                }
            }

            var tracker = new FileVersionTracker(fileStream);
            try {
                tracker.storage.LoadNewValues();
                return tracker;
            } catch (Exception) {
                // If an exception occurred, we are going to try to recover from it by reseting it.
                // reset file length to 0
                fileStream.SetLength(0);
                tracker.Dispose();
                if (!isFirstPass) {
                    throw;
                }
            }

            isFirstPass = false;
        }
    }

    public ObjectId ComputeFileHash(string filePath) {
        var inputVersionKey = new FileVersionKey(filePath);
        storage.LoadNewValues();

        // Perform a lock per file as it can be expensive to compute 
        // them at the same time (for large file)
        object? versionLock;
        lock (locks) {
            if (!locks.TryGetValue(inputVersionKey, out versionLock)) {
                versionLock = new();
                locks.Add(inputVersionKey, versionLock);
            }
        }

        ObjectId hash;
        lock (versionLock) {
            if (!storage.TryGetValue(inputVersionKey, out hash)) {
                // TODO: we might want to allow retries, timeout, etc. since file processed here are files currently being edited by user
                try {
                    using var fileStream = File.OpenRead(filePath);
                    using (var stream = new DigestStream(Stream.Null)) {
                        fileStream.CopyTo(stream);
                        hash = stream.CurrentHash;
                    }
                } catch (Exception ex) {
                    Log.Debug(ex, "Cannot calculate hash for file [{FilePath}]", filePath);
                }

                storage[inputVersionKey] = hash;
            }
        }

        return hash;
    }

    public void Dispose() {
        storage.Dispose();
    }
}
