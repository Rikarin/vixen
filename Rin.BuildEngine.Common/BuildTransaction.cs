using Rin.Core.Serialization.Serialization.Contents;
using Rin.Core.Storage;

namespace Rin.BuildEngine.Common;

class BuildTransaction {
    readonly Dictionary<ObjectUrl, ObjectId> transactionOutputObjects = new();
    readonly IContentIndexMap contentIndexMap;
    readonly IEnumerable<IReadOnlyDictionary<ObjectUrl, OutputObject>> outputObjectsGroups;

    public BuildTransaction(
        IContentIndexMap contentIndexMap,
        IEnumerable<IReadOnlyDictionary<ObjectUrl, OutputObject>> outputObjectsGroups
    ) {
        this.contentIndexMap = contentIndexMap;
        this.outputObjectsGroups = outputObjectsGroups;
    }

    public IEnumerable<KeyValuePair<ObjectUrl, ObjectId>> GetTransactionIdMap() => transactionOutputObjects;

    public IEnumerable<KeyValuePair<string, ObjectId>> SearchValues(
        Func<KeyValuePair<string, ObjectId>, bool> predicate
    ) {
        lock (transactionOutputObjects) {
            return transactionOutputObjects.Select(x => new KeyValuePair<string, ObjectId>(x.Key.Path, x.Value))
                .Where(predicate)
                .ToList();
        }
    }

    public bool TryGetValue(string url, out ObjectId objectId) {
        var objUrl = new ObjectUrl(UrlType.Content, url);

        // Lock TransactionAssetIndexMap
        lock (transactionOutputObjects) {
            if (transactionOutputObjects.TryGetValue(objUrl, out objectId)) {
                return true;
            }

            foreach (var outputObjects in outputObjectsGroups) {
                // Lock underlying EnumerableBuildStep.OutputObjects
                lock (outputObjects) {
                    if (outputObjects.TryGetValue(objUrl, out var outputObject)) {
                        objectId = outputObject.ObjectId;
                        return true;
                    }
                }
            }

            // Check asset index map (if set)
            if (contentIndexMap != null) {
                if (contentIndexMap.TryGetValue(url, out objectId)) {
                    return true;
                }
            }
        }

        objectId = ObjectId.Empty;
        return false;
    }

    internal class DatabaseContentIndexMap : IContentIndexMap {
        readonly BuildTransaction buildTransaction;

        public ObjectId this[string url] {
            get {
                if (!TryGetValue(url, out var objectId)) {
                    throw new KeyNotFoundException();
                }

                return objectId;
            }
            set {
                lock (buildTransaction.transactionOutputObjects) {
                    buildTransaction.transactionOutputObjects[new(UrlType.Content, url)] = value;
                }
            }
        }

        public DatabaseContentIndexMap(BuildTransaction buildTransaction) {
            this.buildTransaction = buildTransaction;
        }

        public bool TryGetValue(string url, out ObjectId objectId) => buildTransaction.TryGetValue(url, out objectId);

        public bool Contains(string url) => TryGetValue(url, out _);

        public IEnumerable<KeyValuePair<string, ObjectId>> SearchValues(
            Func<KeyValuePair<string, ObjectId>, bool> predicate
        ) =>
            buildTransaction.SearchValues(predicate);

        public void WaitPendingOperations() { }

        public IEnumerable<KeyValuePair<string, ObjectId>> GetMergedIdMap() => throw new NotImplementedException();

        public void Dispose() { }
    }
}
