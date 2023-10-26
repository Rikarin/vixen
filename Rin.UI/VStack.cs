namespace Rin.UI;

public class VStack : View {
    readonly View[] items;

    public VStack(View[] items) {
        this.items = items;
    }

    public override void Render() {
        foreach (var item in items) {
            item.Render();
        }
        
        base.Render();
    }
}