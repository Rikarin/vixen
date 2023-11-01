using Rin.Core.Serialization.Storage;
using Rin.Core.Storage;
using Rin.Core.TODO;

namespace Rin.BuildEngine.Common;

public interface IExecuteContext : IPrepareContext {
    CancellationTokenSource CancellationTokenSource { get; }
    ObjectDatabase ResultMap { get; }
    Dictionary<string, string> Variables { get; }

    void ScheduleBuildStep(BuildStep step);
    IEnumerable<IReadOnlyDictionary<ObjectUrl, OutputObject>> GetOutputObjectsGroups();
    CommandBuildStep IsCommandCurrentlyRunning(ObjectId commandHash);
    void NotifyCommandBuildStepStarted(CommandBuildStep commandBuildStep, ObjectId commandHash);
    void NotifyCommandBuildStepFinished(CommandBuildStep commandBuildStep, ObjectId commandHash);
}
