using Vixen.Core.Diagnostics;
using Vixen.Core.Serialization.Contents;
using Vixen.Core.Storage;

namespace Vixen.BuildEngine.Common;

public interface ICommandContext {
    Command CurrentCommand { get; }
    LoggerResult Logger { get; }

    IEnumerable<IReadOnlyDictionary<ObjectUrl, OutputObject>> GetOutputObjectsGroups();
    void RegisterInputDependency(ObjectUrl url);
    void RegisterOutput(ObjectUrl url, ObjectId hash);

    // TODO??
    // void RegisterCommandLog(IEnumerable<ILogMessage> logMessages);

    void AddTag(ObjectUrl url, string tag);
}
