using Rin.Core.Yaml.Events;
using System.Diagnostics;

namespace Rin.Core.Yaml.Serialization;

/// <summary>
///     Represents a scalar node in the YAML document.
/// </summary>
[DebuggerDisplay("{Value}")]
public class YamlScalarNode : YamlNode {
    /// <summary>
    ///     Gets or sets the value of the node.
    /// </summary>
    /// <value>The value.</value>
    public string Value { get; set; }

    /// <summary>
    ///     Gets or sets the style of the node.
    /// </summary>
    /// <value>The style.</value>
    public ScalarStyle Style { get; set; }

    /// <summary>
    ///     Gets all nodes from the document, starting on the current node.
    /// </summary>
    public override IEnumerable<YamlNode> AllNodes {
        get { yield return this; }
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="YamlScalarNode" /> class.
    /// </summary>
    /// <param name="events">The events.</param>
    /// <param name="state">The state.</param>
    internal YamlScalarNode(EventReader events, DocumentLoadingState state) {
        var scalar = events.Expect<Scalar>();
        Load(scalar, state);
        Value = scalar.Value;
        Style = scalar.Style;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="YamlScalarNode" /> class.
    /// </summary>
    public YamlScalarNode() { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="YamlScalarNode" /> class.
    /// </summary>
    /// <param name="value">The value.</param>
    public YamlScalarNode(string value) {
        Value = value;
    }

    /// <summary>
    ///     Accepts the specified visitor by calling the appropriate Visit method on it.
    /// </summary>
    /// <param name="visitor">
    ///     A <see cref="IYamlVisitor" />.
    /// </param>
    public override void Accept(IYamlVisitor visitor) {
        visitor.Visit(this);
    }

    /// <summary />
    public override bool Equals(object? other) {
        var obj = other as YamlScalarNode;
        return obj != null && Equals(obj) && SafeEquals(Value, obj.Value);
    }

    /// <summary>
    ///     Serves as a hash function for a particular type.
    /// </summary>
    /// <returns>
    ///     A hash code for the current <see cref="T:System.Object" />.
    /// </returns>
    public override int GetHashCode() =>
        CombineHashCodes(
            base.GetHashCode(),
            GetHashCode(Value)
        );

    /// <summary>
    ///     Performs an implicit conversion from <see cref="System.String" /> to <see cref="YamlScalarNode" />.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator YamlScalarNode(string value) => new(value);

    /// <summary>
    ///     Performs an explicit conversion from <see cref="YamlScalarNode" /> to <see cref="System.String" />.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the conversion.</returns>
    public static explicit operator string(YamlScalarNode value) => value.Value;

    /// <summary>
    ///     Returns a <see cref="System.String" /> that represents this instance.
    /// </summary>
    /// <returns>
    ///     A <see cref="System.String" /> that represents this instance.
    /// </returns>
    public override string ToString() => Value;

    /// <summary>
    ///     Resolves the aliases that could not be resolved when the node was created.
    /// </summary>
    /// <param name="state">The state of the document.</param>
    internal override void ResolveAliases(DocumentLoadingState state) {
        throw new NotSupportedException("Resolving an alias on a scalar node does not make sense");
    }

    /// <summary>
    ///     Saves the current node to the specified emitter.
    /// </summary>
    /// <param name="emitter">The emitter where the node is to be saved.</param>
    /// <param name="state">The state.</param>
    internal override void Emit(IEmitter emitter, EmitterState state) {
        emitter.Emit(new Scalar(Anchor, Tag, Value, Style, string.IsNullOrEmpty(Tag), false));
    }
}
