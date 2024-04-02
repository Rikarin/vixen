using Vixen.Core.Serialization.Contents;
using Vixen.Core.Storage;

namespace Vixen.BuildEngine.Common;

public class CommandResultEntry {
    public Dictionary<ObjectUrl, ObjectId> InputDependencyVersions = new();

    /// <summary>
    ///     Output object ids as saved in the object database.
    /// </summary>
    public Dictionary<ObjectUrl, ObjectId> OutputObjects = new();

    /// <summary>
    ///     Log messages corresponding to the execution of the command.
    /// </summary>
    ///  TODO
    // public List<SerializableLogMessage> LogMessages;

    /// <summary>
    ///     Tags added for a given URL.
    /// </summary>
    public List<KeyValuePair<ObjectUrl, string>> TagSymbols = new();
}
