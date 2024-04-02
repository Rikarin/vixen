namespace Vixen.InputSystem;

public abstract class GamePadDeviceBase : IGamePadDevice {
    readonly HashSet<GamePadButton> releasedButtons = new();
    readonly HashSet<GamePadButton> pressedButtons = new();
    readonly HashSet<GamePadButton> downButtons = new();
    int index;

    public abstract string Name { get; }
    public abstract Guid Id { get; }
    public abstract Guid ProductId { get; }
    public abstract GamePadState State { get; }
    public bool CanChangeIndex { get; protected set; } = true;
    public int Priority { get; set; }

    public int Index {
        get => index;
        set {
            if (!CanChangeIndex) {
                throw new InvalidOperationException("This GamePad's index can not be changed");
            }

            SetIndexInternal(value, false);
        }
    }

    public IReadOnlySet<GamePadButton> PressedButtons => pressedButtons;
    public IReadOnlySet<GamePadButton> ReleasedButtons => releasedButtons;
    public IReadOnlySet<GamePadButton> DownButtons => downButtons;

    public abstract IInputSource Source { get; }

    public abstract void Update(List<InputEvent> inputEvents);
    public abstract void SetVibration(float smallLeft, float smallRight, float largeLeft, float largeRight);

    public event EventHandler<GamePadIndexChangedEventArgs> IndexChanged;

    protected void SetIndexInternal(int newIndex, bool isDeviceSideChange = true) {
        if (index != newIndex) {
            index = newIndex;
            IndexChanged?.Invoke(this, new() { Index = newIndex, IsDeviceSideChange = isDeviceSideChange });
        }
    }

    /// <summary>
    ///     Clears previous Pressed/Released states
    /// </summary>
    protected void ClearButtonStates() {
        pressedButtons.Clear();
        releasedButtons.Clear();
    }

    /// <summary>
    ///     Updates Pressed/Released/Down collections
    /// </summary>
    protected void UpdateButtonState(GamePadButtonEvent evt) {
        if (evt.IsDown && !downButtons.Contains(evt.Button)) {
            pressedButtons.Add(evt.Button);
            downButtons.Add(evt.Button);
        } else if (!evt.IsDown && downButtons.Contains(evt.Button)) {
            releasedButtons.Add(evt.Button);
            downButtons.Remove(evt.Button);
        }
    }
}
