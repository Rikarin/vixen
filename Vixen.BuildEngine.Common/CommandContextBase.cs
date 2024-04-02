using Vixen.Core.Diagnostics;
using Vixen.Core.Serialization.Contents;
using Vixen.Core.Storage;

namespace Vixen.BuildEngine.Common;

public abstract class CommandContextBase : ICommandContext {
    protected internal readonly CommandResultEntry ResultEntry;
    public Command CurrentCommand { get; }
    public abstract LoggerResult Logger { get; }

    protected CommandContextBase(Command command, BuilderContext builderContext) {
        CurrentCommand = command;
        ResultEntry = new();
    }

    public abstract IEnumerable<IReadOnlyDictionary<ObjectUrl, OutputObject>> GetOutputObjectsGroups();

    public abstract ObjectId ComputeInputHash(UrlType type, string filePath);

    public void RegisterInputDependency(ObjectUrl url) {
        ResultEntry.InputDependencyVersions.Add(url, ComputeInputHash(url.Type, url.Path));
    }

    public void RegisterOutput(ObjectUrl url, ObjectId hash) {
        ResultEntry.OutputObjects.Add(url, hash);
    }

    // TODO: fix
    // public void RegisterCommandLog(IEnumerable<ILogMessage> logMessages) {
    //     foreach (var message in logMessages) {
    //         ResultEntry.LogMessages.Add(
    //             message as SerializableLogMessage ?? new SerializableLogMessage((LogMessage)message)
    //         );
    //     }
    // }

    public void AddTag(ObjectUrl url, string tag) {
        ResultEntry.TagSymbols.Add(new(url, tag));
    }
}
