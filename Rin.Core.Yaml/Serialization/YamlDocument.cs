using Rin.Core.Yaml.Events;
using System.Diagnostics;
using System.Globalization;

namespace Rin.Core.Yaml.Serialization;

/// <summary>
///     Represents an YAML document.
/// </summary>
public class YamlDocument {
    /// <summary>
    ///     Gets or sets the root node.
    /// </summary>
    /// <value>The root node.</value>
    public YamlNode? RootNode { get; private set; }

    /// <summary>
    ///     Gets all nodes from the document.
    /// </summary>
    public IEnumerable<YamlNode> AllNodes => RootNode.AllNodes;

    /// <summary>
    ///     Initializes a new instance of the <see cref="YamlDocument" /> class.
    /// </summary>
    public YamlDocument(YamlNode rootNode) {
        RootNode = rootNode;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="YamlDocument" /> class with a single scalar node.
    /// </summary>
    public YamlDocument(string rootNode) {
        RootNode = new YamlScalarNode(rootNode);
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="YamlDocument" /> class.
    /// </summary>
    /// <param name="events">The events.</param>
    internal YamlDocument(EventReader events) {
        var state = new DocumentLoadingState();

        events.Expect<DocumentStart>();

        while (!events.Accept<DocumentEnd>()) {
            Debug.Assert(RootNode == null);
            RootNode = YamlNode.ParseNode(events, state);

            if (RootNode is YamlAliasNode) {
                throw new YamlException();
            }
        }

        state.ResolveAliases();

#if DEBUG
        foreach (var node in AllNodes) {
            if (node is YamlAliasNode) {
                throw new InvalidOperationException("Error in alias resolution.");
            }
        }
#endif

        events.Expect<DocumentEnd>();
    }

    /// <summary>
    ///     Accepts the specified visitor by calling the appropriate Visit method on it.
    /// </summary>
    /// <param name="visitor">
    ///     A <see cref="IYamlVisitor" />.
    /// </param>
    public void Accept(IYamlVisitor visitor) {
        visitor.Visit(this);
    }

    void AssignAnchors() {
        var visitor = new AnchorAssigningVisitor();
        visitor.AssignAnchors(this);
    }

    /// <summary>
    ///     Visitor that assigns anchors to nodes that are referenced more than once but have no anchor.
    /// </summary>
    class AnchorAssigningVisitor : YamlVisitor {
        readonly HashSet<string> existingAnchors = new();
        readonly Dictionary<YamlNode, bool> visitedNodes = new(new YamlNodeIdentityEqualityComparer());

        public void AssignAnchors(YamlDocument document) {
            existingAnchors.Clear();
            visitedNodes.Clear();

            document.Accept(this);

            var random = new Random();
            foreach (var visitedNode in visitedNodes) {
                if (visitedNode.Value) {
                    string anchor;
                    do {
                        anchor = random.Next().ToString(CultureInfo.InvariantCulture);
                    } while (existingAnchors.Contains(anchor));

                    existingAnchors.Add(anchor);

                    visitedNode.Key.Anchor = anchor;
                }
            }
        }

        void VisitNode(YamlNode node) {
            if (string.IsNullOrEmpty(node.Anchor)) {
                if (visitedNodes.TryGetValue(node, out var isDuplicate)) {
                    if (!isDuplicate) {
                        visitedNodes[node] = true;
                    }
                } else {
                    visitedNodes.Add(node, false);
                }
            } else {
                existingAnchors.Add(node.Anchor);
            }
        }

        protected override void Visit(YamlScalarNode scalar) {
            VisitNode(scalar);
        }

        protected override void Visit(YamlMappingNode mapping) {
            VisitNode(mapping);
        }

        protected override void Visit(YamlSequenceNode sequence) {
            VisitNode(sequence);
        }
    }

    internal void Save(IEmitter emitter, bool isDocumentEndImplicit) {
        AssignAnchors();

        emitter.Emit(new DocumentStart());
        RootNode.Save(emitter, new());
        emitter.Emit(new DocumentEnd(isDocumentEndImplicit));
    }
}
