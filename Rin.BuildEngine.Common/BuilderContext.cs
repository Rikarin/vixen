using Rin.Core.Storage;

namespace Rin.BuildEngine.Common;

public class BuilderContext {
    internal readonly Dictionary<ObjectId, CommandBuildStep> CommandsInProgress = new();

    public CommandBuildStep.TryExecuteRemoteDelegate TryExecuteRemote { get; }

    internal FileVersionTracker InputHashes { get; private set; }

    public BuilderContext(FileVersionTracker inputHashes, CommandBuildStep.TryExecuteRemoteDelegate tryExecuteRemote) {
        InputHashes = inputHashes;
        TryExecuteRemote = tryExecuteRemote;
    }
}
