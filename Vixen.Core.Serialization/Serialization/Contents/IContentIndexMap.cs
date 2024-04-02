using Vixen.Core.Storage;

namespace Vixen.Core.Serialization.Serialization.Contents;

public interface IContentIndexMap : IDisposable {
    ObjectId this[string url] { get; set; }
    
    bool TryGetValue(string url, out ObjectId objectId);
    bool Contains(string url);
    IEnumerable<KeyValuePair<string, ObjectId>> SearchValues(Func<KeyValuePair<string, ObjectId>, bool> predicate);
    IEnumerable<KeyValuePair<string, ObjectId>> GetMergedIdMap();
}
