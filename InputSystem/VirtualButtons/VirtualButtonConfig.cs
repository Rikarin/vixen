using Rin.Core.Collections;
using System.Collections.Specialized;

namespace Rin.InputSystem.VirtualButtons;

/// <summary>
///     Describes the configuration composed by a collection of <see cref="VirtualButtonBinding" />.
/// </summary>
public class VirtualButtonConfig : TrackingCollection<VirtualButtonBinding> {
    readonly Dictionary<object, List<VirtualButtonBinding>> mapBindings;

    /// <summary>
    ///     Gets the binding names.
    /// </summary>
    /// <value>The binding names.</value>
    public IEnumerable<object> BindingNames => mapBindings.Keys;

    /// <summary>
    ///     Initializes a new instance of the <see cref="VirtualButtonConfig" /> class.
    /// </summary>
    public VirtualButtonConfig() {
        mapBindings = new();
        CollectionChanged += Bindings_CollectionChanged;
    }

    public virtual float GetValue(InputManager inputManager, object name) {
        var value = 0.0f;
        if (mapBindings.TryGetValue(name, out var bindingsPerName)) {
            foreach (var virtualButtonBinding in bindingsPerName) {
                var newValue = virtualButtonBinding.GetValue(inputManager);
                if (Math.Abs(newValue) > Math.Abs(value)) {
                    value = newValue;
                }
            }
        }

        return value;
    }

    void Bindings_CollectionChanged(object sender, TrackingCollectionChangedEventArgs e) {
        var virtualButtonBinding = (VirtualButtonBinding)e.Item;
        switch (e.Action) {
            case NotifyCollectionChangedAction.Add:
                AddBinding(virtualButtonBinding);
                break;
            case NotifyCollectionChangedAction.Remove:
                RemoveBinding(virtualButtonBinding);
                break;
        }
    }

    void AddBinding(VirtualButtonBinding virtualButtonBinding) {
        if (!mapBindings.TryGetValue(virtualButtonBinding.Name, out var bindingsPerName)) {
            bindingsPerName = new();
            mapBindings.Add(virtualButtonBinding.Name, bindingsPerName);
        }

        bindingsPerName.Add(virtualButtonBinding);
    }

    void RemoveBinding(VirtualButtonBinding virtualButtonBinding) {
        if (mapBindings.TryGetValue(virtualButtonBinding.Name, out var bindingsPerName)) {
            bindingsPerName.Remove(virtualButtonBinding);
        }
    }
}
