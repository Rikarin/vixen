using Serilog;

namespace Rin.Core.UI;

public class ForEach<T> : View {
    readonly IEnumerable<T> children;
    readonly Func<T, View> callback;

    public ForEach(IEnumerable<T> children, Func<T, View> callback) {
        this.children = children;
        this.callback = callback;
    }

    public override void Render() {
        foreach (var child in children) {
            callback(child).Render();
        }
    }
}
