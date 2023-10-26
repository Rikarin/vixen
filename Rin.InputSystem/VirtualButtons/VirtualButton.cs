using System.Reflection;

namespace Rin.InputSystem.VirtualButtons;

/// <summary>
///     Describes a virtual button (a key from a keyboard, a mouse button, an axis of a joystick...etc.).
/// </summary>
public abstract partial class VirtualButton : IVirtualButton {
    internal const int TypeIdMask = 0x0FFFFFFF;

    /// <summary>
    ///     Unique Id for a particular button <see cref="Type" />.
    /// </summary>
    public readonly int Id;

    /// <summary>
    ///     The short name of this button.
    /// </summary>
    public readonly string ShortName;

    /// <summary>
    ///     Type of this button.
    /// </summary>
    public readonly VirtualButtonType Type;

    /// <summary>
    ///     A boolean indicating whether this button supports positive and negative value.
    /// </summary>
    public readonly bool IsPositiveAndNegative;

    protected readonly int Index;
    static readonly Dictionary<int, VirtualButton> mapIp = new();
    static readonly Dictionary<string, VirtualButton> mapName = new();
    static readonly List<VirtualButton> registered = new();
    static IReadOnlyCollection<VirtualButton> registeredReadOnly;

    string? name;

    /// <summary>
    ///     The full name of this button.
    /// </summary>
    public string Name => name ??= BuildButtonName();

    /// <summary>
    ///     Gets all registered <see cref="VirtualButton" />.
    /// </summary>
    /// <value>The registered virtual buttons.</value>
    public static IReadOnlyCollection<VirtualButton> Registered {
        get {
            EnsureInitialize();
            return registeredReadOnly;
        }
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="VirtualButton" /> class.
    /// </summary>
    /// <param name="shortName">The name.</param>
    /// <param name="type">The type.</param>
    /// <param name="id">The id.</param>
    /// <param name="isPositiveAndNegative">if set to <c>true</c> [is positive and negative].</param>
    protected VirtualButton(string shortName, VirtualButtonType type, int id, bool isPositiveAndNegative = false) {
        Id = (int)type | id;
        Type = type;
        ShortName = shortName;
        IsPositiveAndNegative = isPositiveAndNegative;
        Index = Id & TypeIdMask;
    }

    public override string ToString() => $"{Name}";

    /// <summary>
    ///     Implements the + operator to combine to <see cref="VirtualButton" />.
    /// </summary>
    /// <param name="left">The left virtual button.</param>
    /// <param name="right">The right virtual button.</param>
    /// <returns>A set containting the two virtual buttons.</returns>
    public static IVirtualButton operator +(IVirtualButton left, VirtualButton right) {
        if (left == null) {
            return right;
        }

        return right == null ? left : new VirtualButtonGroup { left, right };
    }

    /// <summary>
    ///     Finds a virtual button by the specified name.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>An instance of VirtualButton or null if no match.</returns>
    public static VirtualButton? Find(string name) {
        EnsureInitialize();
        mapName.TryGetValue(name, out var virtualButton);
        return virtualButton;
    }

    /// <summary>
    ///     Finds a virtual button by the specified id.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <returns>An instance of VirtualButton or null if no match.</returns>
    public static VirtualButton? Find(int id) {
        EnsureInitialize();
        mapIp.TryGetValue(id, out var virtualButton);
        return virtualButton;
    }

    public abstract float GetValue(InputManager manager);

    public abstract bool IsDown(InputManager manager);
    public abstract bool IsPressed(InputManager manager);
    public abstract bool IsReleased(InputManager manager);

    static void EnsureInitialize() {
        lock (mapIp) {
            if (mapIp.Count == 0) {
                RegisterFromType(typeof(Keyboard));
                RegisterFromType(typeof(GamePad));
                RegisterFromType(typeof(Mouse));
                registeredReadOnly = registered;
            }
        }
    }

    static void RegisterFromType(Type type) {
        foreach (var fieldInfo in type.GetTypeInfo().DeclaredFields) {
            if (fieldInfo.IsStatic && typeof(VirtualButton).IsAssignableFrom(fieldInfo.FieldType)) {
                Register((VirtualButton)fieldInfo.GetValue(null));
            }
        }
    }

    static void Register(VirtualButton virtualButton) {
        if (!mapIp.ContainsKey(virtualButton.Id)) {
            mapIp.Add(virtualButton.Id, virtualButton);
            registered.Add(virtualButton);
        }

        mapName.TryAdd(virtualButton.Name, virtualButton);
    }

    protected virtual string BuildButtonName() => $"{Type}.{ShortName}";
}
