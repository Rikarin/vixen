using Rin.Core.Yaml.Tokens;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Rin.Core.Yaml;

/// <summary>
///     Collection of <see cref="TagDirective" />
/// </summary>
public class TagDirectiveCollection : KeyedCollection<string, TagDirective> {
    /// <summary>
    ///     Initializes a new instance of the <see cref="TagDirectiveCollection" /> class.
    /// </summary>
    public TagDirectiveCollection() { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="TagDirectiveCollection" /> class.
    /// </summary>
    /// <param name="tagDirectives">Initial content of the collection.</param>
    public TagDirectiveCollection(IEnumerable<TagDirective> tagDirectives) {
        foreach (var tagDirective in tagDirectives) {
            Add(tagDirective);
        }
    }

    /// <summary>
    ///     Gets a value indicating whether the collection contains a directive with the same handle
    /// </summary>
    public new bool Contains(TagDirective directive) => Contains(GetKeyForItem(directive));

    /// <summary />
    protected override string GetKeyForItem(TagDirective item) => item.Handle;
}
