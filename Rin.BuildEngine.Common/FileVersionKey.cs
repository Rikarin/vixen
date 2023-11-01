using Rin.Core;

namespace Rin.BuildEngine.Common;

[DataContract]
public record struct FileVersionKey {
    public string Path;
    public DateTime LastModifiedDate;
    public long FileSize;

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
