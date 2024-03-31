using Rin.Core.Reflection.MemberDescriptors;
using Rin.Core.Reflection.TypeDescriptors;

namespace Rin.Core.Yaml.Serialization.Serializers;

/// <summary>
///     Default implementation for <see cref="IObjectSerializerBackend" />
/// </summary>
public class DefaultObjectSerializerBackend : IObjectSerializerBackend {
    public virtual DataStyle GetStyle(ref ObjectContext objectContext) {
        var context = objectContext.SerializerContext;

        // Resolve the style, use default style if not defined.
        // First pop style of current member being serialized.
        var style = objectContext.Style;

        // If no style yet defined
        if (style != DataStyle.Any) {
            return style;
        }

        // Try to get the style from this serializer
        style = objectContext.Descriptor.Style;

        // In case of any style, allow to emit a flow sequence depending on Settings LimitPrimitiveFlowSequence.
        // Apply this only for primitives
        if (style == DataStyle.Any) {
            var isPrimitiveElementType = false;
            // TODO(Jiu): Verify commented out collection usage
            // var collectionDescriptor = objectContext.Descriptor as CollectionDescriptor;
            var count = 0;
            // if (collectionDescriptor != null) {
            //     isPrimitiveElementType = PrimitiveDescriptor.IsPrimitive(collectionDescriptor.ElementType);
            //     count = collectionDescriptor.GetCollectionCount(objectContext.Instance);
            // } else {
            if (objectContext.Descriptor is ArrayDescriptor arrayDescriptor) {
                isPrimitiveElementType = PrimitiveDescriptor.IsPrimitive(arrayDescriptor.ElementType);
                count = ((Array)objectContext.Instance)?.Length ?? -1;
            }
            // }

            style = objectContext.Instance == null
                || count >= objectContext.SerializerContext.Settings.LimitPrimitiveFlowSequence
                || !isPrimitiveElementType
                    ? DataStyle.Normal
                    : DataStyle.Compact;
        }

        // If not defined, get the default style
        if (style == DataStyle.Any) {
            style = context.Settings.DefaultStyle;

            // If default style is set to Any, set it to Block by default.
            if (style == DataStyle.Any) {
                style = DataStyle.Normal;
            }
        }

        return style;
    }

    /// <inheritdoc />
    public virtual string ReadMemberName(ref ObjectContext objectContext, string memberName, out bool skipMember) {
        skipMember = false;
        return memberName;
    }

    /// <inheritdoc />
    public virtual object ReadMemberValue(
        ref ObjectContext objectContext,
        IMemberDescriptor memberDescriptor,
        object memberValue,
        Type memberType
    ) {
        var memberObjectContext = new ObjectContext(
            objectContext.SerializerContext,
            memberValue,
            objectContext.SerializerContext.FindTypeDescriptor(memberType)
        );
        return ReadYaml(ref memberObjectContext);
    }

    /// <inheritdoc />
    public virtual object ReadCollectionItem(ref ObjectContext objectContext, object value, Type itemType, int index) {
        var itemObjectContext = new ObjectContext(
            objectContext.SerializerContext,
            value,
            objectContext.SerializerContext.FindTypeDescriptor(itemType)
        );
        return ReadYaml(ref itemObjectContext);
    }

    /// <inheritdoc />
    public virtual object ReadDictionaryKey(ref ObjectContext objectContext, Type keyType) {
        var keyObjectContext = new ObjectContext(
            objectContext.SerializerContext,
            null,
            objectContext.SerializerContext.FindTypeDescriptor(keyType)
        );
        return ReadYaml(ref keyObjectContext);
    }

    /// <inheritdoc />
    public virtual object ReadDictionaryValue(ref ObjectContext objectContext, Type valueType, object key) {
        var valueObjectContext = new ObjectContext(
            objectContext.SerializerContext,
            null,
            objectContext.SerializerContext.FindTypeDescriptor(valueType)
        );
        return ReadYaml(ref valueObjectContext);
    }

    /// <inheritdoc />
    public virtual void WriteMemberName(ref ObjectContext objectContext, IMemberDescriptor member, string name) {
        // Emit the key name
        objectContext.Writer.Emit(
            new ScalarEventInfo(name, typeof(string)) {
                RenderedValue = name, IsPlainImplicit = true, Style = ScalarStyle.Plain
            }
        );
    }

    /// <inheritdoc />
    public virtual void WriteMemberValue(
        ref ObjectContext objectContext,
        IMemberDescriptor memberDescriptor,
        object memberValue,
        Type memberType
    ) {
        // Push the style of the current member
        var memberObjectContext = new ObjectContext(
            objectContext.SerializerContext,
            memberValue,
            objectContext.SerializerContext.FindTypeDescriptor(memberType),
            objectContext.ParentTypeDescriptor,
            memberDescriptor
        ) { Style = memberDescriptor.Style, ScalarStyle = memberDescriptor.ScalarStyle };
        WriteYaml(ref memberObjectContext);
    }

    /// <inheritdoc />
    public virtual void WriteCollectionItem(ref ObjectContext objectContext, object item, Type itemType, int index) {
        var itemObjectContext = new ObjectContext(
            objectContext.SerializerContext,
            item,
            objectContext.SerializerContext.FindTypeDescriptor(itemType)
        );
        WriteYaml(ref itemObjectContext);
    }

    /// <inheritdoc />
    public virtual void WriteDictionaryKey(ref ObjectContext objectContext, object key, Type keyType) {
        var itemObjectContext = new ObjectContext(
            objectContext.SerializerContext,
            key,
            objectContext.SerializerContext.FindTypeDescriptor(keyType)
        );
        WriteYaml(ref itemObjectContext);
    }

    /// <inheritdoc />
    public virtual void WriteDictionaryValue(
        ref ObjectContext objectContext,
        object key,
        object value,
        Type valueType
    ) {
        var itemObjectContext = new ObjectContext(
            objectContext.SerializerContext,
            value,
            objectContext.SerializerContext.FindTypeDescriptor(valueType)
        );
        WriteYaml(ref itemObjectContext);
    }

    /// <inheritdoc />
    public virtual bool ShouldSerialize(IMemberDescriptor member, ref ObjectContext objectContext) =>
        member.ShouldSerialize(objectContext.Instance, objectContext.ParentTypeMemberDescriptor);

    protected object ReadYaml(ref ObjectContext objectContext) {
        var node = objectContext.SerializerContext.Reader.Parser.Current;
        try {
            return objectContext.SerializerContext.Serializer.ObjectSerializer.ReadYaml(ref objectContext);
        } catch (YamlException) {
            throw;
        } catch (Exception ex) {
            ex = ex.Unwrap();
            throw new YamlException(node, ex);
        }
    }

    protected void WriteYaml(ref ObjectContext objectContext) {
        objectContext.SerializerContext.Serializer.ObjectSerializer.WriteYaml(ref objectContext);
    }
}
