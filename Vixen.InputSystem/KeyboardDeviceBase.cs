namespace Vixen.InputSystem;

/// <summary>
///     Base class for keyboard devices
/// </summary>
public abstract class KeyboardDeviceBase : IKeyboardDevice {
    // TODO: property?
    public readonly Dictionary<Key, int> KeyRepeats = new();
    protected readonly List<KeyEvent> Events = new();
    readonly HashSet<Key> pressedKeys = new();
    readonly HashSet<Key> releasedKeys = new();
    readonly HashSet<Key> downKeys = new();

    public IReadOnlySet<Key> PressedKeys => pressedKeys;
    public IReadOnlySet<Key> ReleasedKeys => releasedKeys;
    public IReadOnlySet<Key> DownKeys => downKeys;

    public abstract string Name { get; }
    public abstract Guid Id { get; }
    public int Priority { get; set; }
    public abstract IInputSource Source { get; }

    public virtual void Update(List<InputEvent> inputEvents) {
        pressedKeys.Clear();
        releasedKeys.Clear();

        // Fire events
        foreach (var keyEvent in Events) {
            inputEvents.Add(keyEvent);

            if (keyEvent != null) {
                if (keyEvent.IsDown) {
                    pressedKeys.Add(keyEvent.Key);
                } else {
                    releasedKeys.Add(keyEvent.Key);
                }
            }
        }

        Events.Clear();
    }

    public void HandleKeyDown(Key key) {
        // Increment repeat count on subsequent down events
        if (KeyRepeats.TryGetValue(key, out var repeatCount)) {
            KeyRepeats[key] = ++repeatCount;
        } else {
            KeyRepeats.Add(key, repeatCount);
            downKeys.Add(key);
        }

        var keyEvent = InputEventPool<KeyEvent>.GetOrCreate(this);
        keyEvent.IsDown = true;
        keyEvent.Key = key;
        keyEvent.RepeatCount = repeatCount;
        Events.Add(keyEvent);
    }

    public void HandleKeyUp(Key key) {
        // Prevent duplicate up events
        if (!KeyRepeats.ContainsKey(key)) {
            return;
        }

        KeyRepeats.Remove(key);
        downKeys.Remove(key);
        var keyEvent = InputEventPool<KeyEvent>.GetOrCreate(this);
        keyEvent.IsDown = false;
        keyEvent.Key = key;
        keyEvent.RepeatCount = 0;
        Events.Add(keyEvent);
    }
}
