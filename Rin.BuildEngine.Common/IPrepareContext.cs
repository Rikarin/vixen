using Rin.Core.Storage;
using Rin.Core.TODO;
using Serilog;

namespace Rin.BuildEngine.Common;

public interface IPrepareContext {
    ILogger Logger { get; }
    ObjectId ComputeInputHash(UrlType type, string filePath);
}
