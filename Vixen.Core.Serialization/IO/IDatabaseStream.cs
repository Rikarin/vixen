using Vixen.Core.Storage;

namespace Vixen.Core.Serialization.IO;

public interface IDatabaseStream {
    ObjectId ObjectId { get; }
}
