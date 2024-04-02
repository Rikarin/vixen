using Vixen.Core.Storage;

namespace Vixen.Core.IO;

public interface IDatabaseStream {
    ObjectId ObjectId { get; }
}
