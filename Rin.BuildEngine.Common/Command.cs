using Rin.Core.Serialization;
using Rin.Core.Serialization.Binary;
using Rin.Core.Storage;
using Rin.Core.TODO;
using System.Reflection;

namespace Rin.BuildEngine.Common;

public abstract class Command {
    /// <summary>
    ///     The command cache version, should be bumped when binary serialization format changes (so that cache gets
    ///     invalidated)
    /// </summary>
    protected const int CommandCacheVersion = 1;

    /// <summary>
    ///     Cancellation Token. Must be checked frequently by the <see cref="DoCommandOverride" /> implementation in order to
    ///     interrupt the command while running
    /// </summary>
    public CancellationToken CancellationToken { get; }

    public Func<IEnumerable<ObjectUrl>>? InputFilesGetter { get; }

    /// <summary>
    ///     Title (short description) of the command
    /// </summary>
    public abstract string Title { get; }

    /// <summary>
    ///     The object this command writes (if any).
    /// </summary>
    public virtual string? OutputLocation => null;

    /// <summary>
    ///     Safeguard to ensure inheritance will always call base.PreCommand
    /// </summary>
    internal bool BasePreCommandCalled { get; private set; }

    /// <summary>
    ///     Safeguard to ensure inheritance will always call base.PostCommand
    /// </summary>
    internal bool BasePostCommandCalled { get; private set; }

    /// <summary>
    ///     The method that indirectly call <see cref="DoCommandOverride" /> to execute the actual command code.
    ///     It is called by the current <see cref="Builder" /> when the command is triggered
    /// </summary>
    /// <param name="commandContext"></param>
    public Task<ResultStatus> DoCommand(ICommandContext commandContext) {
        if (CancellationToken.IsCancellationRequested) {
            return Task.FromResult(ResultStatus.Cancelled);
        }

        return DoCommandOverride(commandContext);
    }

    public virtual void PreCommand(ICommandContext commandContext) {
        // Safeguard, will throw an exception if a inherited command does not call base.PreCommand
        BasePreCommandCalled = true;
    }

    public virtual void PostCommand(ICommandContext commandContext, ResultStatus status) {
        // Safeguard, will throw an exception if a inherited command does not call base.PostCommand
        BasePostCommandCalled = true;

        // TODO
        // commandContext.RegisterCommandLog(commandContext.Logger.Messages);
    }

    public Command Clone() {
        var copy = (Command)Activator.CreateInstance(GetType())!;
        foreach (var property in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
            if (property.GetSetMethod() != null) {
                var value = property.GetValue(this);
                property.SetValue(copy, value);
            }
        }

        return copy;
    }

    /// <inheritdoc />
    public abstract override string ToString();

    /// <summary>
    ///     Gets the list of input files (that can be deduced without running the command, only from command parameters).
    /// </summary>
    /// <returns></returns>
    public virtual IEnumerable<ObjectUrl> GetInputFiles() =>
        InputFilesGetter?.Invoke() ?? Enumerable.Empty<ObjectUrl>();

    /// <summary>
    ///     Check some conditions that determine if the command should be executed. This method may not be called if some
    ///     previous check determined that it already needs to be executed.
    /// </summary>
    /// <returns>true if the command should be executed</returns>
    public virtual bool ShouldForceExecution() => false;

    public virtual bool ShouldSpawnNewProcess() => false;

    /// <summary>
    ///     Callback called by <see cref="Builder.CancelBuild" />. It can be useful for commands in a blocking call that can be
    ///     unblocked from here.
    /// </summary>
    public virtual void Cancel() {
        // Do nothing by default
    }

    public void ComputeCommandHash(Stream stream, IPrepareContext prepareContext) {
        var writer =
            new BinarySerializationWriter(stream) {
                Context = { SerializerSelector = SerializerSelector.AssetWithReuse }
            };

        writer.Write(CommandCacheVersion);

        // Compute assembly hash
        ComputeAssemblyHash(writer);

        // Compute parameters hash
        ComputeParameterHash(writer);

        // Compute static input files hash (parameter dependent)
        ComputeInputFilesHash(writer, prepareContext);
    }

    /// <summary>
    ///     The method to override containing the actual command code. It is called by the <see cref="DoCommand" /> function
    /// </summary>
    /// <param name="commandContext"></param>
    protected abstract Task<ResultStatus> DoCommandOverride(ICommandContext commandContext);

    protected virtual void ComputeParameterHash(BinarySerializationWriter writer) {
        // Do nothing by default
    }

    protected void ComputeInputFilesHash(BinarySerializationWriter writer, IPrepareContext prepareContext) {
        foreach (var inputFile in GetInputFiles()) {
            var hash = prepareContext.ComputeInputHash(inputFile.Type, inputFile.Path);
            if (hash == ObjectId.Empty) {
                writer.UnderlyingStream.WriteByte(0);
            } else {
                writer.UnderlyingStream.Write((byte[])hash, 0, ObjectId.HashSize);
            }
        }
    }

    protected virtual void ComputeAssemblyHash(BinarySerializationWriter writer) {
        // Use binary format version (bumping it forces everything to be reevaluated)
        writer.Write(DataSerializer.BinaryFormatVersion);

        // Gets the hash of the assembly of the command
        //writer.Write(AssemblyHash.ComputeAssemblyHash(GetType().Assembly));
    }

    /// <summary>
    ///     Computes the command hash. If an error occurred, the hash is <see cref="ObjectId.Empty" />
    /// </summary>
    /// <param name="prepareContext">The prepare context.</param>
    /// <returns>Hash of the command.</returns>
    internal ObjectId ComputeCommandHash(IPrepareContext prepareContext) {
        var stream = new DigestStream(Stream.Null);
        try {
            ComputeCommandHash(stream, prepareContext);
            return stream.CurrentHash;
        } catch (Exception ex) {
            prepareContext.Logger.Error(
                ex,
                "Unexpected error while computing the command hash for [{Name}]",
                GetType().Name
            );
        }

        return ObjectId.Empty;
    }
}
