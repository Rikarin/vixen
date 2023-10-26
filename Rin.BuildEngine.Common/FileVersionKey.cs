namespace Rin.BuildEngine.Common;

public readonly record struct FileVersionKey {
    public readonly string Path;
    public readonly DateTime LastModifiedDate;
    public readonly long FileSize;

    public FileVersionKey(string path) {
        Path = path;
        LastModifiedDate = DateTime.MinValue;
        FileSize = -1;

        if (File.Exists(path)) {
            LastModifiedDate = File.GetLastWriteTime(path);
            FileSize = new FileInfo(path).Length;
        }
    }
}
