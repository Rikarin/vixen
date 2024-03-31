namespace Rin.Core.Yaml.Serialization;

/// <summary>
///     The exception that is thrown when an alias references an anchor that does not exist.
/// </summary>
public class AnchorNotFoundException : YamlException {
    /// <summary>
    ///     Gets or sets the anchor alias.
    /// </summary>
    /// <value>The anchor alias.</value>
    public string Alias { get; private set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="AnchorNotFoundException" /> class.
    /// </summary>
    /// <param name="anchorAlias">The anchor alias.</param>
    public AnchorNotFoundException(string anchorAlias) {
        Alias = anchorAlias;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="AnchorNotFoundException" /> class.
    /// </summary>
    /// <param name="anchorAlias">The anchor alias.</param>
    /// <param name="message">The message.</param>
    public AnchorNotFoundException(string anchorAlias, string message) : base(message) {
        Alias = anchorAlias;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="AnchorNotFoundException" /> class.
    /// </summary>
    /// <param name="anchorAlias">The anchor alias.</param>
    /// <param name="start">The start.</param>
    /// <param name="end">The end.</param>
    /// <param name="message">The message.</param>
    public AnchorNotFoundException(string anchorAlias, Mark start, Mark end, string message) :
        base(start, end, message) {
        Alias = anchorAlias;
    }
}
