using Rin.Core.Yaml.Events;
using System.Collections;
using System.Diagnostics;
using System.Text;

namespace Rin.Core.Yaml.Serialization;

/// <summary>
///     Represents a sequence node in the YAML document.
/// </summary>
[DebuggerDisplay("Count = {Children.Count}")]
public class YamlSequenceNode : YamlNode, IEnumerable<YamlNode> {
    /// <summary>
    ///     Gets the collection of child nodes.
    /// </summary>
    /// <value>The children.</value>
    public IList<YamlNode> Children { get; } = new List<YamlNode>();

    /// <summary>
    ///     Gets or sets the style of the node.
    /// </summary>
    /// <value>The style.</value>
    public DataStyle Style { get; set; }

    /// <summary>
    ///     Gets all nodes from the document, starting on the current node.
    /// </summary>
    public override IEnumerable<YamlNode> AllNodes {
        get {
            yield return this;

            foreach (var child in Children) {
                foreach (var node in child.AllNodes) {
                    yield return node;
                }
            }
        }
    }


    /// <summary>
    ///     Initializes a new instance of the <see cref="YamlSequenceNode" /> class.
    /// </summary>
    /// <param name="events">The events.</param>
    /// <param name="state">The state.</param>
    internal YamlSequenceNode(EventReader events, DocumentLoadingState state) {
        var sequence = events.Expect<SequenceStart>();
        Load(sequence, state);
        Style = sequence.Style;

        var hasUnresolvedAliases = false;
        while (!events.Accept<SequenceEnd>()) {
            var child = ParseNode(events, state);
            Children.Add(child);
            hasUnresolvedAliases |= child is YamlAliasNode;
        }

        if (hasUnresolvedAliases) {
            state.AddNodeWithUnresolvedAliases(this);
        }
#if DEBUG
        else {
            foreach (var child in Children) {
                if (child is YamlAliasNode) {
                    throw new InvalidOperationException("Error in alias resolution.");
                }
            }
        }
#endif

        events.Expect<SequenceEnd>();
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="YamlSequenceNode" /> class.
    /// </summary>
    public YamlSequenceNode() { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="YamlSequenceNode" /> class.
    /// </summary>
    public YamlSequenceNode(params YamlNode[] children)
        : this((IEnumerable<YamlNode>)children) { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="YamlSequenceNode" /> class.
    /// </summary>
    public YamlSequenceNode(IEnumerable<YamlNode> children) {
        foreach (var child in children) {
            Children.Add(child);
        }
    }

    /// <summary>
    ///     Adds the specified child to the <see cref="Children" /> collection.
    /// </summary>
    /// <param name="child">The child.</param>
    public void Add(YamlNode child) {
        Children.Add(child);
    }

    /// <summary>
    ///     Adds a scalar node to the <see cref="Children" /> collection.
    /// </summary>
    /// <param name="child">The child.</param>
    public void Add(string child) {
        Children.Add(new YamlScalarNode(child));
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
    public override bool Equals(object other) {
        var obj = other as YamlSequenceNode;
        if (obj == null || !Equals(obj) || Children.Count != obj.Children.Count) {
            return false;
        }

        for (var i = 0; i < Children.Count; ++i) {
            if (!SafeEquals(Children[i], obj.Children[i])) {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     Serves as a hash function for a particular type.
    /// </summary>
    /// <returns>
    ///     A hash code for the current <see cref="T:System.Object" />.
    /// </returns>
    public override int GetHashCode() {
        var hashCode = base.GetHashCode();

        foreach (var item in Children) {
            hashCode = CombineHashCodes(hashCode, GetHashCode(item));
        }

        return hashCode;
    }

    /// <summary>
    ///     Returns a <see cref="System.String" /> that represents this instance.
    /// </summary>
    /// <returns>
    ///     A <see cref="System.String" /> that represents this instance.
    /// </returns>
    public override string ToString() {
        var text = new StringBuilder("[ ");

        foreach (var child in Children) {
            if (text.Length > 2) {
                text.Append(", ");
            }

            text.Append(child);
        }

        text.Append(" ]");

        return text.ToString();
    }

    #region IEnumerable<YamlNode> Members

    /// <summary />
    public IEnumerator<YamlNode> GetEnumerator() => Children.GetEnumerator();

    #endregion

    #region IEnumerable Members

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion

    /// <summary>
    ///     Resolves the aliases that could not be resolved when the node was created.
    /// </summary>
    /// <param name="state">The state of the document.</param>
    internal override void ResolveAliases(DocumentLoadingState state) {
        for (var i = 0; i < Children.Count; ++i) {
            if (Children[i] is YamlAliasNode) {
                Children[i] = state.GetNode(Children[i].Anchor, true, Children[i].Start, Children[i].End);
            }
        }
    }

    /// <summary>
    ///     Saves the current node to the specified emitter.
    /// </summary>
    /// <param name="emitter">The emitter where the node is to be saved.</param>
    /// <param name="state">The state.</param>
    internal override void Emit(IEmitter emitter, EmitterState state) {
        emitter.Emit(new SequenceStart(Anchor, Tag, string.IsNullOrEmpty(Tag), Style));
        foreach (var node in Children) {
            node.Save(emitter, state);
        }

        emitter.Emit(new SequenceEnd());
    }
}
