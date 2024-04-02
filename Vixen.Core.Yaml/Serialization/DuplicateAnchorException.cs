namespace Vixen.Core.Yaml.Serialization;

/// <summary>
///     The exception that is thrown when a duplicate anchor is detected.
/// </summary>
public class DuplicateAnchorException : YamlException {
    /// <summary>
    ///     Initializes a new instance of the <see cref="DuplicateAnchorException" /> class.
    /// </summary>
    public DuplicateAnchorException() { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DuplicateAnchorException" /> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public DuplicateAnchorException(string message) : base(message) { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DuplicateAnchorException" /> class.
    /// </summary>
    public DuplicateAnchorException(Mark start, Mark end, string message) : base(start, end, message) { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DuplicateAnchorException" /> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="inner">The inner.</param>
    public DuplicateAnchorException(string message, Exception inner) : base(message, inner) { }
}
