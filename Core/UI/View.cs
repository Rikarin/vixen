namespace Rin.Core.UI;

public abstract partial class View {
    Dictionary<Type, object> configurations = new();
    
    // View? cachedBody;
    ContextMenuView? contextMenuView;

    /// <summary>
    ///     Implement this to render stuff. State is not persisted across frames.
    /// </summary>
    protected virtual View? Body { get; }

    public View Hidden() => this;

    public View Style(PushConfiguration style) {
        configurations[style.GetType()] = style;
        return this;
    }

    public View ContextMenu(params View[] contextMenu) {
        contextMenuView = new(contextMenu);
        return this;
    }

    public virtual void Render() {
        // TODO: improve caching. Not updated when tree is updated dynamically
        // cachedBody ??= Body;
        // cachedBody?.Render();
        Body?.Render();
        contextMenuView?.Render();
    }

    protected T? GetConfiguration<T>() where T : PushConfiguration {
        if (configurations.TryGetValue(typeof(T), out var value)) {
            return value as T;
        }

        return null;
    }
}

// TODO: date picker, color picker

public partial class View {
    // Menu
    public MenuItem MenuItem(string label, Action? onSubmit = null) => new(label, onSubmit);
    public Menu Menu(string label, params View[] children) => new(label, children);
    
    // Basic
    public EmptyView Empty() => new();
    public ForEach<T> ForEach<T>(IEnumerable<T> children, Func<T, View> callback) => new(children, callback);
    public Divider Divider() => new();
    public Spacing Spacing() => new();
    public Text Text(string text) => new(text);
    
    // Stacks
    public VStack VStack(params View[] children) => new(children);
    public HStack HStack(params View[] children) => new(children);
    public CollapsingView CollapsingView(string label, View child) => new(label, child);


    public TextField TextField(State<string> text) => new(null, text);
    public TextField TextField(string label, State<string> text) => new(label, text);
    
    // Controls
    public Button Button(string label, Action? onSubmit = null) => new(label, onSubmit);
    public Toggle Toggle(State<bool> isOn) => new(null, isOn);
    public Toggle Toggle(string label, State<bool> isOn) => new(label, isOn);
    public Picker Picker(State<string> selection, params string[] options) => new(null, selection, options);
    public Picker Picker(string label, State<string> selection, params string[] options) =>
        new(label, selection, options);
    
    // Sliders
    public Slider Slider(State<float> value) => new SliderFloat(value, new(0, 100));
    public Slider Slider(State<float> value, Range range) => new SliderFloat(value, range);
    public Slider Slider(State<float> value, Range range, string format) => new SliderFloat(value, range, format);
    public Slider Slider(State<int> value) => new SliderInt(value, new(0, 100));
    public Slider Slider(State<int> value, Range range) => new SliderInt(value, range);
    public Slider Slider(State<int> value, Range range, string format) => new SliderInt(value, range, format);
    
    // Drag
    public Drag Drag(State<float> value) => new DragFloat(value, new(0, 0), 1);
    public Drag Drag(State<float> value, float speed) => new DragFloat(value, new(0, 0), speed);
    public Drag Drag(State<float> value, float speed, Range range) => new DragFloat(value, range, speed);
    public Drag Drag(State<float> value, float speed, Range range, string format) => new DragFloat(value, range, speed, format);
    public Drag Drag(State<int> value) => new DragInt(value, new(0, 0), 1);
    public Drag Drag(State<int> value, float speed) => new DragInt(value, new(0, 0), speed);
    public Drag Drag(State<int> value, float speed, Range range) => new DragInt(value, range, speed);
    public Drag Drag(State<int> value, float speed, Range range, string format) => new DragInt(value, range, speed, format);

    // Grid
    public Grid Grid(params View[] children) => new(children);
    public GridRow GridRow(params View[] children) => new(children);

    // Table
    public Table<T> Table<T>(T[] data, params TableColumn<T>[] columns) => new(data, columns);
    public TableColumn<T> TableColumn<T>(string header, Func<T, string> formatter) => new(header, formatter);
    public TableColumn<T> TableColumn<T>(string header, Func<T, View> context) => new(header, context);
}
