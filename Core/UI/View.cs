using Rin.Core.General;
using Serilog;
using System.Drawing;

namespace Rin.Core.UI;

public partial class View {
    public View Hidden() => this;
}

public class Button : View {
    public Button Background(Color color) => this;

    public Button Font(Font font) => this;

    public Button OnSubmit(Action action) => this;
}

public class TestLayout : View { }

public class TestView : View {
    public void Render() {
        // TestLayout();
        // Button();

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
}

public partial class View {
    public Button Button(string text) => new();

    public static TestLayout TestLayout(params View[] children) => new();
}
