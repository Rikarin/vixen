using System.Diagnostics;
using Vixen.Core.Diagnostics;

namespace Vixen.Core.IO;

public partial class DirectoryWatcher {
    readonly Dictionary<string, DirectoryWatcherItem> watchers = new(StringComparer.CurrentCultureIgnoreCase);

    void InitializeInternal() {
        watcherCheckThread = new(SafeAction.Wrap(RunCheckWatcher)) { IsBackground = true, Name = "DirectoryWatcher" };
        watcherCheckThread.Start();
    }

    void DisposeInternal() {
        foreach (var watcher in watchers.Values) {
            if (watcher.Watcher != null) {
                DisposeNativeWatcher(watcher.Watcher);
            }

            watcher.Watcher = null;
        }

        watchers.Clear();
    }

    List<string> GetTrackedDirectoriesInternal() {
        List<string> directories;
        lock (watchers) {
            directories = ListTrackedDirectories().Select(pair => pair.Key).ToList();
        }

        directories.Sort();
        return directories;
    }

    void TrackInternal(string path) {
        var info = GetDirectoryInfoFromPath(path);
        if (info == null) {
            return;
        }

        lock (watchers) {
            Track(info, true);
        }
    }

    void UnTrackInternal(string path) {
        var info = GetDirectoryInfoFromPath(path);
        if (info == null) {
            return;
        }

        lock (watchers) {
            if (!watchers.TryGetValue(info.FullName, out var watcher)) {
                return;
            }

            UnTrack(watcher, true);
        }
    }

    void RunCheckWatcher() {
        try {
            while (!exitThread) {
                // TODO should use a wait on an event in order to cancel it more quickly instead of a blocking Thread.Sleep
                Thread.Sleep(SleepBetweenWatcherCheck);

                lock (watchers) {
                    var list = ListTrackedDirectories().ToList();
                    foreach (var watcherKeyPath in list) {
                        if (!watcherKeyPath.Value.IsPathExist()) {
                            UnTrack(watcherKeyPath.Value, true);
                            OnModified(
                                this,
                                new FileEvent(
                                    FileEventChangeType.Deleted,
                                    Path.GetFileName(watcherKeyPath.Value.Path),
                                    watcherKeyPath.Value.Path
                                )
                            );
                        }
                    }

                    // If no more directories are tracked, clear completely the watchers tree
                    if (!ListTrackedDirectories().Any()) {
                        watchers.Clear();
                    }
                }
            }
        } catch (Exception ex) {
            Trace.WriteLine($"Unexpected end of thread {ex}");
        }
    }

    IEnumerable<KeyValuePair<string, DirectoryWatcherItem>> ListTrackedDirectories() {
        return watchers.Where(pair => pair.Value.Watcher != null);
    }

    DirectoryInfo? GetDirectoryInfoFromPath(string path) {
        if (path == null) {
            throw new ArgumentNullException(nameof(path));
        }

        path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), path));

        // 1) Extract directory information from path
        DirectoryInfo info;
        if (File.Exists(path)) {
            path = Path.GetDirectoryName(path)!;
        }

        if (Directory.Exists(path)) {
            info = new(path.ToLowerInvariant());
        } else {
            return null;
        }

        return info;
    }

    IEnumerable<DirectoryWatcherItem> ListTracked(IEnumerable<DirectoryInfo> directories) {
        foreach (var directoryInfo in directories) {
            if (watchers.TryGetValue(directoryInfo.FullName, out var watcher)) {
                yield return watcher;
            }
        }
    }

    IEnumerable<DirectoryWatcherItem> ListTrackedChildren(DirectoryWatcherItem watcher) =>
        ListTracked(watcher.ListChildrenDirectories());

    int CountTracked(IEnumerable<DirectoryInfo> directories) {
        return ListTracked(directories).Count(watcher => watcher.Watcher != null);
    }

    DirectoryWatcherItem Track(DirectoryInfo info, bool watcherNode) {
        if (watchers.TryGetValue(info.FullName, out var watcher)) {
            if (watcher.Watcher == null && watcherNode) {
                watcher.Watcher = CreateFileSystemWatcher(watcher.Path);
            }

            watcher.TrackCount++;
            return watcher;
        }

        var parent = info.Parent != null ? Track(info.Parent, false) : null;

        if (parent != null && watcherNode) {
            if (parent.Watcher != null) {
                return parent;
            }

            var childrenDirectoryList = parent.ListChildrenDirectories().ToList();
            var countTracked = CountTracked(childrenDirectoryList);

            var newCount = countTracked + 1;
            if (newCount == childrenDirectoryList.Count && newCount > 1) {
                UnTrack(parent, false);
                parent.Watcher = CreateFileSystemWatcher(parent.Path);
                return parent;
            }
        }

        watcher = new(info) { Parent = parent };
        if (watcherNode) {
            watcher.Watcher = CreateFileSystemWatcher(watcher.Path);
        }

        watchers.Add(watcher.Path, watcher);

        watcher.TrackCount++;
        return watcher;
    }

    void UnTrack(DirectoryWatcherItem watcher, bool removeWatcherFromGlobals) {
        foreach (var child in ListTrackedChildren(watcher)) {
            UnTrack(child, true);
        }

        watcher.TrackCount--;

        if (watcher.TrackCount == 0) {
            if (watcher.Watcher != null) {
                DisposeNativeWatcher(watcher.Watcher);
                watcher.Watcher = null;
            }

            watcher.Parent = null;

            if (removeWatcherFromGlobals) {
                watchers.Remove(watcher.Path);
            }
        }
    }

    void DisposeNativeWatcher(FileSystemWatcher watcher) {
        watcher.EnableRaisingEvents = false;
        watcher.Changed -= OnModified;
        watcher.Created -= OnModified;
        watcher.Deleted -= OnModified;
        watcher.Renamed -= OnModified;
        watcher.Error -= WatcherOnError;
        watcher.Dispose();
    }

    void WatcherOnError(object sender, ErrorEventArgs errorEventArgs) {
        try {
            lock (watchers) {
                var watcher = watchers.Values.FirstOrDefault(item => item.Watcher == sender);
                if (watcher != null) {
                    // Remove a specific watcher if there was any error with it
                    UnTrack(watcher, true);
                }
            }
        } catch (Exception ex) {
            Trace.WriteLine($"Unexpected exception in WatcherOnError: {ex}");
        }
    }

    void OnModified(object sender, FileSystemEventArgs e) {
        lock (watchers) {
            if (e.ChangeType == WatcherChangeTypes.Deleted && watchers.TryGetValue(e.FullPath, out var watcher)) {
                UnTrack(watcher, true);
            }
        }

        if (Modified != null) {
            if (e.ChangeType == WatcherChangeTypes.Renamed) {
                var renamedEventArgs = e as RenamedEventArgs;
                OnModified(this, new FileRenameEvent(e.Name, e.FullPath, renamedEventArgs!.OldFullPath));
            } else {
                OnModified(this, new FileEvent((FileEventChangeType)e.ChangeType, e.Name, e.FullPath));
            }
        }
    }

    protected FileSystemWatcher CreateFileSystemWatcher(string directory) {
        var watcher = new FileSystemWatcher {
            Path = directory,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
            Filter = FileFilter,
            IncludeSubdirectories = true
        };

        watcher.Changed += OnModified;
        watcher.Created += OnModified;
        watcher.Deleted += OnModified;
        watcher.Renamed += OnModified;
        watcher.Error += WatcherOnError;
        watcher.EnableRaisingEvents = true;

        return watcher;
    }

    [DebuggerDisplay("Active: {IsActive}, Path: {Path}")]
    sealed class DirectoryWatcherItem {
        public DirectoryWatcherItem? Parent;

        public string Path { get; private set; }

        public int TrackCount { get; set; }

        public FileSystemWatcher? Watcher { get; set; }

        bool IsActive => Watcher != null;

        public DirectoryWatcherItem(DirectoryInfo path) {
            Path = path.FullName.ToLowerInvariant();
        }

        public bool IsPathExist() => Directory.Exists(Path);

        public IEnumerable<DirectoryInfo> ListChildrenDirectories() {
            var info = new DirectoryInfo(Path);
            try {
                if (info.Exists) {
                    return info.EnumerateDirectories();
                }
            } catch (Exception) {
                // An exception can occur if the file is being removed
            }

            return Enumerable.Empty<DirectoryInfo>();
        }
    }
}
