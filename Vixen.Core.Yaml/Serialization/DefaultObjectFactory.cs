using System.Collections;
using Vixen.Core.Reflection.TypeDescriptors;

namespace Vixen.Core.Yaml.Serialization;

/// <summary>
///     Creates objects using Activator.CreateInstance.
/// </summary>
public sealed class DefaultObjectFactory : IObjectFactory {
    static readonly Type[] EmptyTypes = [];

    static readonly Dictionary<Type, Type> DefaultInterfaceImplementations = new() {
        { typeof(IList), typeof(List<object>) },
        { typeof(IDictionary), typeof(Dictionary<object, object>) },
        { typeof(IEnumerable<>), typeof(List<>) },
        { typeof(ICollection<>), typeof(List<>) },
        { typeof(IList<>), typeof(List<>) },
        { typeof(IDictionary<,>), typeof(Dictionary<,>) }
    };

    /// <summary>
    ///     Gets the default implementation for a type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>The type of the implem or the same type as input if there is no default implementation</returns>
    public static Type GetDefaultImplementation(Type type) {
        if (type == null) {
            return null;
        }

        // TODO change this code. Make it configurable?
        if (type.IsInterface) {
            Type? implementationType;
            if (type.IsGenericType) {
                if (DefaultInterfaceImplementations.TryGetValue(
                        type.GetGenericTypeDefinition(),
                        out implementationType
                    )) {
                    type = implementationType.MakeGenericType(type.GetGenericArguments());
                }
            } else {
                if (DefaultInterfaceImplementations.TryGetValue(type, out implementationType)) {
                    type = implementationType;
                }
            }
        }

        return type;
    }

    /// <inheritdoc />
    public object Create(Type type) {
        type = GetDefaultImplementation(type);

        // We can't instantiate primitives or arrays
        if (PrimitiveDescriptor.IsPrimitive(type) || type.IsArray) {
            throw new InstanceCreationException($"Failed to create instance of type '{type}', wrong factory.");
        }

        if (type.GetConstructor(EmptyTypes) != null || type.IsValueType) {
            try {
                return Activator.CreateInstance(type);
            } catch (Exception e) {
                throw new InstanceCreationException(
                    $"'{typeof(Activator)}' failed to create instance of type '{type}', see inner exception.",
                    e
                );
            }
        }

        throw new InstanceCreationException(
            $"Failed to create instance of type '{type}', type does not have a parameterless constructor."
        );
    }

    public class InstanceCreationException : Exception {
        public InstanceCreationException(string message) : base(message) { }
        public InstanceCreationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
