using Serilog;
using Vixen.Core.Diagnostics;
using Vixen.Core.Serialization.Contents;
using Vixen.Core.Storage;

namespace Vixen.BuildEngine.Common;

public abstract class BuildStep {
    internal bool ProcessedDependencies = false;

    BuildStep? parent;

    /// <summary>
    ///     Callback that can transform the <see cref="IExecuteContext.Logger" />.
    /// </summary>
    /// <value>The module.</value>
    public TransformExecuteContextLoggerDelegate TransformExecuteContextLogger { get; set; }

    /// <summary>
    ///     Gets or sets the priority amongst other build steps.
    /// </summary>
    /// <value>
    ///     The priority.
    /// </value>
    public int? Priority { get; set; }

    /// <summary>
    ///     Title of the build step. Intended to be short
    /// </summary>
    public abstract string Title { get; }

    /// <summary>
    ///     Description of the build step. Intended to be longer and more descriptive than the <see cref="Title" />
    /// </summary>
    public string Description => ToString();

    /// <summary>
    ///     The status of the result.
    /// </summary>
    public ResultStatus Status { get; private set; }

    /// <summary>
    ///     Indicate whether this command has already been processed (ie. executed or skipped) by the Builder
    /// </summary>
    public bool Processed => Status != ResultStatus.NotProcessed;

    /// <summary>
    ///     Indicate whether the result corresponds to a successful execution (even if the command has not been triggered)
    /// </summary>
    public bool Succeeded => Status is ResultStatus.Successful or ResultStatus.NotTriggeredWasSuccessful;

    /// <summary>
    ///     Indicate whether the result corresponds to a failed execution (even if the command has not been triggered)
    /// </summary>
    public bool Failed => Status is ResultStatus.Failed or ResultStatus.NotTriggeredPrerequisiteFailed;

    /// <summary>
    ///     A tag property that can contain anything useful for tools based on this build Engine.
    /// </summary>
    public object Tag { get; set; }

    /// <summary>
    ///     List of commands that must be executed prior this one (direct dependence only).
    /// </summary>
    public HashSet<BuildStep> PrerequisiteSteps { get; } = new();

    /// <summary>
    ///     The parent build step, which will be the instigator of the step
    /// </summary>
    public BuildStep? Parent {
        get => parent;
        protected internal set {
            if (parent != null && value != null) {
                throw new InvalidOperationException("BuildStep already has a parent");
            }

            parent = value;
        }
    }

    /// <summary>
    ///     An unique id during a build execution, assigned once the build step is scheduled.
    /// </summary>
    public long ExecutionId { get; internal set; }

    /// <summary>
    ///     Indicate whether all prerequisite commands have been processed
    /// </summary>
    public bool ArePrerequisitesCompleted {
        get { return PrerequisiteSteps.All(x => x.Processed); }
    }

    /// <summary>
    ///     Indicate whether all prerequisite commands have been processed and are in a successful state
    /// </summary>
    public bool ArePrerequisitesSuccessful {
        get { return PrerequisiteSteps.All(x => x.Succeeded); }
    }

    /// <summary>
    ///     Gets the logger for the current build step.
    /// </summary>
    public LoggerResult Logger { get; } = new();

    /// <summary>
    ///     The object this build step write (if any).
    /// </summary>
    public virtual string? OutputLocation => null;

    /// <summary>
    ///     The list of objects generated by this build step.
    /// </summary>
    public virtual IEnumerable<KeyValuePair<ObjectUrl, ObjectId>> OutputObjectIds =>
        throw new NotImplementedException();

    protected BuildStep(ResultStatus status = ResultStatus.NotProcessed) {
        Status = status;
    }

    /// <summary>
    ///     Execute the BuildStep, usually resulting in scheduling tasks in the scheduler
    /// </summary>
    /// <param name="executeContext">The execute context</param>
    /// <param name="builderContext">The builder context</param>
    /// <returns>A task returning <see cref="ResultStatus" /> indicating weither the execution has successed or failed.</returns>
    public abstract Task<ResultStatus> Execute(IExecuteContext executeContext, BuilderContext builderContext);

    /// <summary>
    ///     Clean the build, deleting the command cache which is used to determine wheither a command has already been
    ///     executed, and deleting the output objects if asked.
    /// </summary>
    /// <param name="executeContext">The execute context</param>
    /// <param name="builderContext">The builder context</param>
    /// <param name="deleteOutput">if true, every output object is also deleted, in addition of the command cache.</param>
    public virtual void Clean(IExecuteContext executeContext, BuilderContext builderContext, bool deleteOutput) {
        // By default, do the same as Execute. This will apply for flow control steps (lists, enumerations...)
        // Specific implementation exists for CommandBuildStep
        Execute(executeContext, builderContext);
    }

    public abstract override string ToString();

    public static void LinkBuildSteps(BuildStep parent, BuildStep child) {
        lock (child.PrerequisiteSteps) {
            child.PrerequisiteSteps.Add(parent);
        }
    }

    public Task<BuildStep> ExecutedAsync() {
        // Already processed?
        if (Processed) {
            return Task.FromResult(this);
        }

        var tcs = new TaskCompletionSource<BuildStep>();
        StepProcessed += (sender, e) => tcs.TrySetResult(e.Step);
        return tcs.Task;
    }

    public IEnumerable<IReadOnlyDictionary<ObjectUrl, OutputObject>> GetOutputObjectsGroups() {
        var currentBuildStep = this;
        while (currentBuildStep != null) {
            if (currentBuildStep is ListBuildStep enumBuildStep) {
                yield return enumBuildStep.OutputObjects;
            }

            foreach (var prerequisiteStep in currentBuildStep.PrerequisiteSteps) {
                foreach (var outputObjectsGroup in prerequisiteStep.GetOutputObjectsGroups()) {
                    yield return outputObjectsGroup;
                }
            }

            currentBuildStep = currentBuildStep.Parent;
        }
    }

    public delegate void TransformExecuteContextLoggerDelegate(ref ILogger logger);

    /// <summary>
    ///     Event raised when the command is processed (even if it has been skipped or if it failed)
    /// </summary>
    public event EventHandler<BuildStepEventArgs>? StepProcessed;

    /// <summary>
    ///     Associate the given <see cref="ResultStatus" /> object as the result of the current step and execute the
    ///     <see cref="StepProcessed" /> event.
    /// </summary>
    /// <param name="executeContext">The execute context.</param>
    /// <param name="status">The result status.</param>
    internal void RegisterResult(IExecuteContext executeContext, ResultStatus status) {
        Status = status;

        //executeContext.Logger.Debug("Step timer for {0}: callbacks: {1}ms, total: {2}ms", this, CallbackWatch.ElapsedMilliseconds, MicroThreadWatch.ElapsedMilliseconds);

        if (StepProcessed != null) {
            try {
                var outputObjectsGroups = executeContext.GetOutputObjectsGroups();
                MicroThreadLocalDatabases.MountDatabase(outputObjectsGroups);
                StepProcessed(this, new(this, executeContext.Logger));
            } catch (Exception ex) {
                executeContext.Logger.Error(ex, "Exception in command {Command}", this);
            } finally {
                MicroThreadLocalDatabases.UnmountDatabase();
            }
        }
    }
}
