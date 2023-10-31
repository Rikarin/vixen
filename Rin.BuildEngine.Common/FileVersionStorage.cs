using Rin.Core.Serialization;
using Rin.Core.Serialization.Serializers;
using Rin.Core.Storage;
using System.Text;

namespace Rin.BuildEngine.Common;

/// <summary>
///     Storage used for <see cref="FileVersionKey" /> associated with an <see cref="ObjectId" />.
/// </summary>
[DataSerializerGlobal(typeof(KeyValuePairSerializer<FileVersionKey, ObjectId>))]
public sealed class FileVersionStorage : DictionaryStore<FileVersionKey, ObjectId> {
    /// <summary>
    ///     Initializes a new instance of the <see cref="FileVersionStorage" /> class.
    /// </summary>
    /// <param name="stream">The localStream.</param>
    public FileVersionStorage(Stream stream) : base(stream) { }

    /// <summary>
    ///     Compacts the specified storage path.
    /// </summary>
    /// <param name="storagePath">The storage path.</param>
    /// <returns><c>true</c> if the storage path was successfully compacted, <c>false</c> otherwise.</returns>
    public static bool Compact(string storagePath) {
        FileStream fileStreamExclusive;
        try {
            fileStreamExclusive = new(storagePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        } catch (Exception) {
            return false;
        }

        try {
            using (var localTracker =
                   new FileVersionStorage(fileStreamExclusive) { UseTransaction = true, AutoLoadNewValues = false }) {
                localTracker.LoadNewValues();
                var latestVersion =
                    new Dictionary<string, KeyValuePair<FileVersionKey, ObjectId>>(StringComparer.OrdinalIgnoreCase);
                foreach (var keyValue in localTracker.GetValues()) {
                    var filePath = keyValue.Key.Path;
                    if (!latestVersion.TryGetValue(filePath, out var previousKeyValue)
                        || keyValue.Key.LastModifiedDate > previousKeyValue.Key.LastModifiedDate) {
                        latestVersion[filePath] = keyValue;
                    }
                }

                localTracker.Reset();
                localTracker.AddValues(latestVersion.Values);
                localTracker.Save();
            }
        } catch (Exception) {
            return false;
        }

        return true;
    }

    protected override List<KeyValuePair<FileVersionKey, ObjectId>> ReadEntries(Stream localStream) {
        // As the FileVersionStorage is not used at runtime but only at build time, it is not currently optimized 
        // TODO: performance of encoding/decoding could be improved using some manual (but much more laborious) code.
        var reader = new StreamReader(localStream, Encoding.UTF8);
        var entries = new List<KeyValuePair<FileVersionKey, ObjectId>>();
        while (reader.ReadLine() is { } line) {
            var values = line.Split('\t');
            if (values.Length != 4) {
                continue;
            }

            // Path: values[0]
            var key = new FileVersionKey { Path = values[0] };
            if (!long.TryParse(values[1], out var dateTime)) {
                throw new InvalidOperationException(
                    $"Unable to decode datetime [{values[1]}] when reading file version index"
                );
            }

            key.LastModifiedDate = new(dateTime);
            if (!long.TryParse(values[2], out key.FileSize)) {
                throw new InvalidOperationException(
                    $"Unable to decode filesize [{values[2]}] when reading file version index"
                );
            }

            var objectIdStr = values[3];
            if (!ObjectId.TryParse(objectIdStr, out ObjectId objectId)) {
                throw new InvalidOperationException(
                    $"Unable to decode ObjectId [{objectIdStr}] when reading file version index"
                );
            }

            var entry = new KeyValuePair<FileVersionKey, ObjectId>(key, objectId);
            entries.Add(entry);
        }

        return entries;
    }

    protected override void WriteEntry(Stream localStream, KeyValuePair<FileVersionKey, ObjectId> value) {
        var key = value.Key;
        var line = $"{key.Path}\t{key.LastModifiedDate.Ticks}\t{key.FileSize}\t{value.Value}\n";
        var bytes = Encoding.UTF8.GetBytes(line);
        localStream.Write(bytes, 0, bytes.Length);
    }
}
