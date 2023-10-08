using Serilog;
using System.Drawing;

namespace Rin.Core.UI;

public abstract partial class View {
    View? cachedBody;
    ContextMenuView? contextMenuView;

    /// <summary>
    /// Implement this to render stuff
    /// </summary>
    protected virtual View? Body { get; }

    public View Hidden() => this;

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
}

public sealed class EmptyView : View { }

public class TestLayout : View { }

public class TestView : View {
    protected override View Body =>
        // @formatter:off
        TestLayout(
            TestLayout(
                Button("foo bar")
                    .Background(Color.Blue)
                    .Font(null!)
                    .OnSubmit(() => Log.Information("called"))
                )
            );
        // @formatter:on
}

public partial class View {
    public EmptyView Empty() => new();
    public ForEach<T> ForEach<T>(IEnumerable<T> children, Func<T, View> callback) => new(children, callback);
    public Button Button(string label, Action? onSubmit = null) => new(label, onSubmit);
    public MenuItem MenuItem(string label, Action? onSubmit = null) => new(label, onSubmit);
    public Separator Separator() => new();
    public Menu Menu(string label, params View[] children) => new(label, children);
    public VerticalView VStack(params View[] children) => new(children);
    public Picker Picker(string label, State<string> selection, params string[] options) => new(label, selection, options);
    public Toggle Toggle(string label, State<bool> isOn) => new(label, isOn);
    public Text Text(string text) => new(text);

    public static TestLayout TestLayout(params View[] children) => new();
}