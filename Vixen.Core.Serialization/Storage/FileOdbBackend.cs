using System.Text;
using Vixen.Core.IO;
using Vixen.Core.Serialization.Contents;

namespace Vixen.Core.Storage;

/// <summary>
///     Object Database Backend (ODB) implementation using <see cref="VirtualFileSystem" />
/// </summary>
public class FileOdbBackend : IOdbBackend {
    const int WriteBufferSize = 1024;
    const string TempDirectory = "/tmp/";

    static readonly object LockOnMove = new();

    // Resolve provider once at initialization
    readonly IVirtualFileProvider virtualFileProvider;
    readonly string vfsRootUrl;
    readonly string vfsTempUrl;
    readonly ContentIndexMap contentIndexMap;

    /// <inheritdoc />
    public IContentIndexMap ContentIndexMap => contentIndexMap;

    public bool IsReadOnly { get; }

    public FileOdbBackend(string vfsRootUrl, string indexName, bool isReadOnly) {
        var resolveProviderResult = VirtualFileSystem.ResolveProvider(vfsRootUrl, true);
        virtualFileProvider = resolveProviderResult.Provider;
        this.vfsRootUrl = resolveProviderResult.Path;
        vfsTempUrl = this.vfsRootUrl + TempDirectory;

        // Ensure directories exists
        if (!virtualFileProvider.DirectoryExists(this.vfsRootUrl)) {
            virtualFileProvider.CreateDirectory(this.vfsRootUrl);
        }

        IsReadOnly = isReadOnly;

        contentIndexMap = !string.IsNullOrEmpty(indexName)
            ? Serialization.Contents.ContentIndexMap.Load(
                vfsRootUrl + VirtualFileSystem.DirectorySeparatorChar + indexName,
                isReadOnly
            )
            : Serialization.Contents.ContentIndexMap.CreateInMemory();
        if (!isReadOnly && !virtualFileProvider.DirectoryExists(vfsTempUrl)) {
            try {
                virtualFileProvider.CreateDirectory(vfsTempUrl);
            } catch (Exception) {
                IsReadOnly = true;
            }
        }
    }

    public void Dispose() {
        contentIndexMap.Dispose();
    }

    /// <inheritdoc />
    public virtual Stream OpenStream(
        ObjectId objectId,
        VirtualFileMode mode = VirtualFileMode.Open,
        VirtualFileAccess access = VirtualFileAccess.Read,
        VirtualFileShare share = VirtualFileShare.Read
    ) {
        var url = BuildUrl(vfsRootUrl, objectId);

        // Try to early exit if file does not exists while opening, so that it doesn't
        // throw a file not found exception for default logic.
        if (!virtualFileProvider.FileExists(url)) {
            if (mode == VirtualFileMode.Open || mode == VirtualFileMode.Truncate) {
                throw new FileNotFoundException();
            }

            // Otherwise, file creation is allowed, so make sure directory exists
            virtualFileProvider.CreateDirectory(ExtractPath(url));
        }

        return virtualFileProvider.OpenStream(url, mode, access, share);
    }

    /// <inheritdoc />
    public virtual int GetSize(ObjectId objectId) {
        var url = BuildUrl(vfsRootUrl, objectId);
        using (var file = virtualFileProvider.OpenStream(url, VirtualFileMode.Open, VirtualFileAccess.Read)) {
            return checked((int)file.Length);
        }
    }

    /// <inheritdoc />
    public virtual bool Exists(ObjectId objectId) {
        var url = BuildUrl(vfsRootUrl, objectId);
        return virtualFileProvider.FileExists(url);
    }

    /// <inheritdoc />
    public virtual ObjectId Write(ObjectId objectId, Stream dataStream, int length, bool forceWrite = false) {
        if (objectId == ObjectId.Empty) {
            // This should be avoided
            using (var digestStream = new DigestStream(Stream.Null)) {
                dataStream.CopyTo(digestStream);
                objectId = digestStream.CurrentHash;
            }

            dataStream.Seek(0, SeekOrigin.Begin);
        }

        var tmpFileName = vfsTempUrl + Guid.NewGuid() + ".tmp";
        var url = BuildUrl(vfsRootUrl, objectId);

        if (!forceWrite && virtualFileProvider.FileExists(url)) {
            return objectId;
        }

        using (var file = virtualFileProvider.OpenStream(
                   tmpFileName,
                   VirtualFileMode.Create,
                   VirtualFileAccess.Write
               )) {
            dataStream.CopyTo(file);
        }

        MoveToDatabase(tmpFileName, objectId, forceWrite);

        return objectId;
    }

    /// <inheritdoc />
    public OdbStreamWriter CreateStream() {
        if (IsReadOnly) {
            throw new InvalidOperationException("Read-only backend.");
        }

        var tmpFileName = vfsTempUrl + Guid.NewGuid() + ".tmp";
        var stream = virtualFileProvider.OpenStream(tmpFileName, VirtualFileMode.Create, VirtualFileAccess.Write);
        return new DigestStream(stream, tmpFileName) { Disposed = x => MoveToDatabase(x.TemporaryName, x.CurrentHash) };
    }

    /// <inheritdoc />
    public void Delete(ObjectId objectId) {
        var url = BuildUrl(vfsRootUrl, objectId);
        virtualFileProvider.FileDelete(url);
    }

    /// <inheritdoc />
    public IEnumerable<ObjectId> EnumerateObjects() {
        foreach (var file in virtualFileProvider.ListFiles(vfsRootUrl, "*", VirtualSearchOption.AllDirectories)) {
            if (file.Length >= 2 + ObjectId.HashStringLength) {
                if (file[file.Length - ObjectId.HashStringLength - 2] != VirtualFileSystem.DirectorySeparatorChar
                    || file[file.Length - ObjectId.HashStringLength + 1] != VirtualFileSystem.DirectorySeparatorChar) {
                    continue;
                }

                var objectIdString = new char[ObjectId.HashStringLength];
                var filePosition = file.Length - ObjectId.HashStringLength - 1;
                for (var i = 0; i < ObjectId.HashStringLength; ++i) {
                    // Skip /
                    if (i == 2) {
                        filePosition++;
                    }

                    objectIdString[i] = file[filePosition++];
                }

                if (ObjectId.TryParse(new(objectIdString), out var objectId)) {
                    yield return objectId;
                }
            }
        }
    }

    public string GetFilePath(ObjectId objectId) => virtualFileProvider.GetAbsolutePath(BuildUrl(vfsRootUrl, objectId));

    public static string BuildUrl(string vfsRootUrl, ObjectId objectId) {
        var id = objectId.ToString();
        var result = new StringBuilder(vfsRootUrl.Length + 2 + ObjectId.HashStringLength);
        result.Append(vfsRootUrl);
        result.Append('/');
        result.Append(id[0]);
        result.Append(id[1]);
        result.Append('/');
        result.Append(id, 2, ObjectId.HashStringLength - 2);

        return result.ToString();
    }

    void MoveToDatabase(string temporaryFilePath, ObjectId objId, bool forceWrite = false) {
        var fileUrl = BuildUrl(vfsRootUrl, objId);

        lock (LockOnMove) {
            var fileExists = virtualFileProvider.FileExists(fileUrl);

            // File may already exists, in this case we decide to not override it.
            if (!fileExists || forceWrite) {
                try {
                    if (fileExists) {
                        // In case we force write, delete old file so that move succeed
                        virtualFileProvider.FileDelete(fileUrl);
                    } else {
                        // Remove the second part of ObjectId to get the path (cf BuildUrl)
                        virtualFileProvider.CreateDirectory(
                            fileUrl.Substring(0, fileUrl.Length - (ObjectId.HashStringLength - 2))
                        );
                    }

                    virtualFileProvider.FileMove(temporaryFilePath, fileUrl);
                } catch (IOException e) {
                    // Ignore only IOException "The destination file already exists."
                    // because other exceptions that we want to catch might inherit from IOException.
                    // This happens if two FileMove were performed at the same time.
                    if (e.GetType() != typeof(IOException)) {
                        throw;
                    }

                    // But we should still clean our temporary file
                    virtualFileProvider.FileDelete(temporaryFilePath);
                }
            } else {
                // But we should still clean our temporary file
                virtualFileProvider.FileDelete(temporaryFilePath);
            }
        }
    }

    static string ExtractPath(string url) => url.Substring(0, url.LastIndexOf('/'));
}
