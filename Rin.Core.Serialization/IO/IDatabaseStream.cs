using Rin.Core.Storage;

namespace Rin.Core.Serialization.IO;

public interface IDatabaseStream {
    ObjectId ObjectId { get; }
}
