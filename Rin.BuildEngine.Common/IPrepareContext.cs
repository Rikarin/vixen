using Rin.Core.Serialization.Serialization.Contents;
using Rin.Core.Storage;
using Serilog;

namespace Rin.BuildEngine.Common;

public interface IPrepareContext {
    ILogger Logger { get; }
    ObjectId ComputeInputHash(UrlType type, string filePath);
}
