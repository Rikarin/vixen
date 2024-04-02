using System.Collections;
using System.Text;
using Vixen.Core.Yaml.Events;

namespace Vixen.Core.Yaml.Serialization;

/// <summary>
///     Represents a mapping node in the YAML document.
/// </summary>
public class YamlMappingNode : YamlNode, IEnumerable<KeyValuePair<YamlNode, YamlNode>> {
    /// <summary>
    ///     Gets the children of the current node.
    /// </summary>
    /// <value>The children.</value>
    public IOrderedDictionary<YamlNode, YamlNode> Children { get; } = new OrderedDictionary<YamlNode, YamlNode>();

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
                foreach (var node in child.Key.AllNodes) {
                    yield return node;
                }

                foreach (var node in child.Value.AllNodes) {
                    yield return node;
                }
            }
        }
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="YamlMappingNode" /> class.
    /// </summary>
    /// <param name="events">The events.</param>
    /// <param name="state">The state.</param>
    internal YamlMappingNode(EventReader events, DocumentLoadingState state) {
        var mapping = events.Expect<MappingStart>();
        Load(mapping, state);
        Style = mapping.Style;

        var hasUnresolvedAliases = false;
        while (!events.Accept<MappingEnd>()) {
            var key = ParseNode(events, state);
            var value = ParseNode(events, state);

            try {
                Children.Add(key, value);
            } catch (ArgumentException err) {
                throw new YamlException(key.Start, key.End, "Duplicate key", err);
            }

            hasUnresolvedAliases |= key is YamlAliasNode || value is YamlAliasNode;
        }

        if (hasUnresolvedAliases) {
            state.AddNodeWithUnresolvedAliases(this);
        }
#if DEBUG
        else {
            foreach (var child in Children) {
                if (child.Key is YamlAliasNode) {
                    throw new InvalidOperationException("Error in alias resolution.");
                }

                if (child.Value is YamlAliasNode) {
                    throw new InvalidOperationException("Error in alias resolution.");
                }
            }
        }
#endif

        events.Expect<MappingEnd>();
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="YamlMappingNode" /> class.
    /// </summary>
    public YamlMappingNode() { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="YamlMappingNode" /> class.
    /// </summary>
    public YamlMappingNode(params KeyValuePair<YamlNode, YamlNode>[] children)
        : this((IEnumerable<KeyValuePair<YamlNode, YamlNode>>)children) { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="YamlMappingNode" /> class.
    /// </summary>
    public YamlMappingNode(IEnumerable<KeyValuePair<YamlNode, YamlNode>> children) {
        foreach (var child in children) {
            Children.Add(child);
        }
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="YamlMappingNode" /> class.
    /// </summary>
    /// <param name="children">A sequence of <see cref="YamlNode" /> where even elements are keys and odd elements are values.</param>
    public YamlMappingNode(params YamlNode[] children) : this((IEnumerable<YamlNode>)children) { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="YamlMappingNode" /> class.
    /// </summary>
    /// <param name="children">A sequence of <see cref="YamlNode" /> where even elements are keys and odd elements are values.</param>
    public YamlMappingNode(IEnumerable<YamlNode> children) {
        using var enumerator = children.GetEnumerator();
        while (enumerator.MoveNext()) {
            var key = enumerator.Current;
            if (!enumerator.MoveNext()) {
                throw new ArgumentException(
                    "When constructing a mapping node with a sequence, the number of elements of the sequence must be even."
                );
            }

            Add(key, enumerator.Current);
        }
    }

    /// <summary>
    ///     Adds the specified mapping to the <see cref="Children" /> collection.
    /// </summary>
    /// <param name="key">The key node.</param>
    /// <param name="value">The value node.</param>
    public void Add(YamlNode key, YamlNode value) {
        Children.Add(key, value);
    }

    /// <summary>
    ///     Adds the specified mapping to the <see cref="Children" /> collection.
    /// </summary>
    /// <param name="key">The key node.</param>
    /// <param name="value">The value node.</param>
    public void Add(string key, YamlNode value) {
        Children.Add(new YamlScalarNode(key), value);
    }

    /// <summary>
    ///     Adds the specified mapping to the <see cref="Children" /> collection.
    /// </summary>
    /// <param name="key">The key node.</param>
    /// <param name="value">The value node.</param>
    public void Add(YamlNode key, string value) {
        Children.Add(key, new YamlScalarNode(value));
    }

    /// <summary>
    ///     Adds the specified mapping to the <see cref="Children" /> collection.
    /// </summary>
    /// <param name="key">The key node.</param>
    /// <param name="value">The value node.</param>
    public void Add(string key, string value) {
        Children.Add(new YamlScalarNode(key), new YamlScalarNode(value));
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
        if (other is not YamlMappingNode obj || !Equals(obj) || Children.Count != obj.Children.Count) {
            return false;
        }

        foreach (var entry in Children) {
            if (!obj.Children.TryGetValue(entry.Key, out var otherNode) || !SafeEquals(entry.Value, otherNode)) {
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

        foreach (var entry in Children) {
            hashCode = CombineHashCodes(hashCode, GetHashCode(entry.Key));
            hashCode = CombineHashCodes(hashCode, GetHashCode(entry.Value));
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
        var text = new StringBuilder("{ ");

        foreach (var child in Children) {
            if (text.Length > 2) {
                text.Append(", ");
            }

            text.Append("{ ").Append(child.Key).Append(", ").Append(child.Value).Append(" }");
        }

        text.Append(" }");
        return text.ToString();
    }

    #region IEnumerable<KeyValuePair<YamlNode,YamlNode>> Members

    /// <summary />
    public IEnumerator<KeyValuePair<YamlNode, YamlNode>> GetEnumerator() => Children.GetEnumerator();

    #endregion

    #region IEnumerable Members

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion

    /// <summary>
    ///     Resolves the aliases that could not be resolved when the node was created.
    /// </summary>
    /// <param name="state">The state of the document.</param>
    internal override void ResolveAliases(DocumentLoadingState state) {
        Dictionary<YamlNode, YamlNode>? keysToUpdate = null;
        Dictionary<YamlNode, YamlNode>? valuesToUpdate = null;
        foreach (var entry in Children) {
            if (entry.Key is YamlAliasNode) {
                keysToUpdate ??= new();
                keysToUpdate.Add(entry.Key, state.GetNode(entry.Key.Anchor, true, entry.Key.Start, entry.Key.End));
            }

            if (entry.Value is YamlAliasNode) {
                valuesToUpdate ??= new();
                valuesToUpdate.Add(
                    entry.Key,
                    state.GetNode(entry.Value.Anchor, true, entry.Value.Start, entry.Value.End)
                );
            }
        }

        if (valuesToUpdate != null) {
            foreach (var entry in valuesToUpdate) {
                Children[entry.Key] = entry.Value;
            }
        }

        if (keysToUpdate != null) {
            foreach (var entry in keysToUpdate) {
                var value = Children[entry.Key];
                Children.Remove(entry.Key);
                Children.Add(entry.Value, value);
            }
        }
    }

    /// <summary>
    ///     Saves the current node to the specified emitter.
    /// </summary>
    /// <param name="emitter">The emitter where the node is to be saved.</param>
    /// <param name="state">The state.</param>
    internal override void Emit(IEmitter emitter, EmitterState state) {
        emitter.Emit(new MappingStart(Anchor, Tag, string.IsNullOrEmpty(Tag), Style));
        foreach (var entry in Children) {
            entry.Key.Save(emitter, state);
            entry.Value.Save(emitter, state);
        }

        emitter.Emit(new MappingEnd());
    }
}
