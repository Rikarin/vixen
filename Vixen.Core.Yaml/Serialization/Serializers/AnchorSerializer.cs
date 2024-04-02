using Vixen.Core.Yaml.Events;

namespace Vixen.Core.Yaml.Serialization.Serializers;

class AnchorSerializer : ChainedSerializer {
    readonly Dictionary<string, object> aliasToObject = new();
    readonly Dictionary<object, string> objectToAlias = new(new IdentityEqualityComparer<object>());

    public bool TryGetAliasValue(string alias, out object value) => aliasToObject.TryGetValue(alias, out value);

    public override object ReadYaml(ref ObjectContext objectContext) {
        var context = objectContext.SerializerContext;
        var reader = context.Reader;
        object? value;

        // Process Anchor alias (*oxxx)
        var alias = reader.Allow<AnchorAlias>();
        if (alias != null) {
            // Return an alias or directly the value
            if (!aliasToObject.TryGetValue(alias.Value, out value)) {
                throw new AnchorNotFoundException(alias.Value, alias.Start, alias.End, "Unable to find alias");
            }

            return value;
        }

        // Test if current node has an anchor &oxxx
        string? anchor = null;
        var nodeEvent = reader.Peek<NodeEvent>();
        if (nodeEvent != null && !string.IsNullOrEmpty(nodeEvent.Anchor)) {
            anchor = nodeEvent.Anchor;
        }

        // Deserialize the current node
        value = base.ReadYaml(ref objectContext);

        // Store Anchor (&oxxx) and override any defined anchor 
        if (anchor != null) {
            aliasToObject[anchor] = value;
        }

        return value;
    }

    public override void WriteYaml(ref ObjectContext objectContext) {
        var value = objectContext.Instance;

        // Only write anchors for object (and not value types)
        var isAnchorable = false;
        if (value != null && !value.GetType().IsValueType) {
            var typeCode = Type.GetTypeCode(value.GetType());
            switch (typeCode) {
                case TypeCode.Object:
                case TypeCode.String:
                    isAnchorable = true;
                    break;
            }
        }

        if (isAnchorable) {
            if (objectToAlias.TryGetValue(value, out var alias)) {
                objectContext.Writer.Emit(new AliasEventInfo(value, value.GetType()) { Alias = alias });
                return;
            }

            alias = $"o{objectContext.SerializerContext.AnchorCount}";
            objectToAlias.Add(value, alias);

            objectContext.Anchor = alias;
            objectContext.SerializerContext.AnchorCount++;
        }

        base.WriteYaml(ref objectContext);
    }
}
