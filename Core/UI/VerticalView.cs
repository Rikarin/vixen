namespace Rin.Core.UI;

public class VerticalView : View {
    readonly View[] items;

    public VerticalView(View[] items) {
        this.items = items;
    }

    public override void Render() {
        foreach (var item in items) {
            item.Render();
        }
    }
}
