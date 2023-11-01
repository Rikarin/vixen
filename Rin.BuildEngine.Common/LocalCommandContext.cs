using Rin.Core.Diagnostics;
using Rin.Core.Storage;
using Rin.Core.TODO;

namespace Rin.BuildEngine.Common;

public class LocalCommandContext : CommandContextBase {
    readonly IExecuteContext executeContext;

    public CommandBuildStep Step { get; protected set; }

    public override LoggerResult Logger { get; }

    public LocalCommandContext(IExecuteContext executeContext, CommandBuildStep step, BuilderContext builderContext) :
        base(step.Command, builderContext) {
        this.executeContext = executeContext;
        // TODO: fix
        // Logger = new ForwardingLoggerResult(executeContext.Logger);
        Step = step;
    }

    public override IEnumerable<IReadOnlyDictionary<ObjectUrl, OutputObject>> GetOutputObjectsGroups() =>
        Step.GetOutputObjectsGroups();

    public override ObjectId ComputeInputHash(UrlType type, string filePath) =>
        executeContext.ComputeInputHash(type, filePath);
}
