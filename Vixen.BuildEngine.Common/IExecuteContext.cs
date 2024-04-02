using Vixen.Core.Serialization.Serialization.Contents;
using Vixen.Core.Serialization.Storage;
using Vixen.Core.Storage;

namespace Vixen.BuildEngine.Common;

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
