using System.Globalization;

namespace Vixen.Core.Yaml.Serialization;

/// <summary>
///     Manages the state of a <see cref="YamlDocument" /> while it is loading.
/// </summary>
class DocumentLoadingState {
    readonly IDictionary<string, YamlNode> anchors = new Dictionary<string, YamlNode>();
    readonly IList<YamlNode> nodesWithUnresolvedAliases = new List<YamlNode>();

    /// <summary>
    ///     Adds the specified node to the anchor list.
    /// </summary>
    /// <param name="node">The node.</param>
    public void AddAnchor(YamlNode node) {
        if (node.Anchor == null) {
            throw new ArgumentException("The specified node does not have an anchor");
        }

        if (anchors.ContainsKey(node.Anchor)) {
            throw new DuplicateAnchorException(
                node.Start,
                node.End,
                string.Format(CultureInfo.InvariantCulture, "The anchor '{0}' already exists", node.Anchor)
            );
        }

        anchors.Add(node.Anchor, node);
    }

    /// <summary>
    ///     Gets the node with the specified anchor.
    /// </summary>
    /// <param name="anchor">The anchor.</param>
    /// <param name="throwException">
    ///     if set to <c>true</c>, the method should throw an exception if there is no node with that
    ///     anchor.
    /// </param>
    /// <param name="start">The start position.</param>
    /// <param name="end">The end position.</param>
    /// <returns></returns>
    public YamlNode GetNode(string anchor, bool throwException, Mark start, Mark end) {
        if (anchors.TryGetValue(anchor, out var target)) {
            return target;
        }

        if (throwException) {
            throw new AnchorNotFoundException(
                anchor,
                start,
                end,
                string.Format(CultureInfo.InvariantCulture, "The anchor '{0}' does not exists", anchor)
            );
        }

        return null;
    }

    /// <summary>
    ///     Adds the specified node to the collection of nodes with unresolved aliases.
    /// </summary>
    /// <param name="node">
    ///     The <see cref="YamlNode" /> that has unresolved aliases.
    /// </param>
    public void AddNodeWithUnresolvedAliases(YamlNode node) {
        nodesWithUnresolvedAliases.Add(node);
    }

    /// <summary>
    ///     Resolves the aliases that could not be resolved while loading the document.
    /// </summary>
    public void ResolveAliases() {
        foreach (var node in nodesWithUnresolvedAliases) {
            node.ResolveAliases(this);

#if DEBUG
            foreach (var child in node.AllNodes) {
                if (child is YamlAliasNode) {
                    throw new InvalidOperationException("Error in alias resolution.");
                }
            }
#endif
        }
    }
}
