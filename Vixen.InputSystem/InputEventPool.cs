using Vixen.Core.Common.Collections;

namespace Vixen.InputSystem;

/// <summary>
///     Pools input events of a given type
/// </summary>
/// <typeparam name="TEventType">The type of event to pool</typeparam>
public static class InputEventPool<TEventType> where TEventType : InputEvent, new() {
    static readonly ThreadLocal<Pool> pool;

    /// <summary>
    ///     The number of events in circulation, if this number keeps increasing, Enqueue is possible not called somewhere
    /// </summary>
    public static int ActiveObjects => pool.Value.ActiveObjects;

    static InputEventPool() {
        pool = new(() => new());
    }

    /// <summary>
    ///     Retrieves a new event that can be used, either from the pool or a new instance
    /// </summary>
    /// <param name="device">The device that generates this event</param>
    /// <returns>An event</returns>
    public static TEventType GetOrCreate(IInputDevice device) => pool.Value.GetOrCreate(device);

    /// <summary>
    ///     Puts a used event back into the pool to be recycled
    /// </summary>
    /// <param name="item">The event to reuse</param>
    public static void Enqueue(TEventType item) {
        pool.Value.Enqueue(item);
    }

    static TEventType CreateEvent() => new();

    /// <summary>
    ///     Pool class, since <see cref="PoolListStruct{T}" /> can not be placed inside <see cref="ThreadLocal{T}" />
    /// </summary>
    class Pool {
        PoolListStruct<TEventType> pool = new(8, CreateEvent);

        public int ActiveObjects => pool.Count;

        public TEventType GetOrCreate(IInputDevice device) {
            var item = pool.Add();
            item.Device = device;
            return item;
        }

        public void Enqueue(TEventType item) {
            item.Device = null;
            pool.Remove(item);
        }
    }
}
