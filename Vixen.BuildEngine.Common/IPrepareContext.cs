using Serilog;
using Vixen.Core.Serialization.Contents;
using Vixen.Core.Storage;

namespace Vixen.BuildEngine.Common;

public interface IPrepareContext {
    ILogger Logger { get; }
    ObjectId ComputeInputHash(UrlType type, string filePath);
}
