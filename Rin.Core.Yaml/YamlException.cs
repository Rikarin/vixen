using Rin.Core.Yaml.Events;

namespace Rin.Core.Yaml;

/// <summary>
///     Base exception that is thrown when the a problem occurs in the SharpYaml library.
/// </summary>
public class YamlException : Exception {
    /// <summary>
    ///     Gets the position in the input stream where the event that originated the exception starts.
    /// </summary>
    public Mark Start { get; private set; }

    /// <summary>
    ///     Gets the position in the input stream where the event that originated the exception ends.
    /// </summary>
    public Mark End { get; private set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="YamlException" /> class.
    /// </summary>
    public YamlException(string? message = null, Exception? inner = null) : base(message, inner) { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="YamlException" /> class.
    /// </summary>
    public YamlException(Mark start, Mark end, string message, Exception? innerException = null)
        : base($"{message} (({start.ToString()}) -> ({end.ToString()}))", innerException) {
        Start = start;
        End = end;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="YamlException" /> class.
    /// </summary>
    public YamlException(ParsingEvent node, Exception innerException) :
        this(
            node.Start,
            node.End,
            $"An exception occured while deserializing node [{node}], see inner exception",
            innerException
        ) { }
}
