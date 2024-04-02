namespace Vixen.Core.Reflection;

/// <summary>
///     This attribute can be either used on class or interfaces to scan for types inheriting from them, or on an attribute
///     to scan for types having this specific attribute.
/// TODO: Not implemented??
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct)]
public class AssemblyScanAttribute : Attribute;
