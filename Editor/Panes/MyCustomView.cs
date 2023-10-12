using Rin.Core.UI;
using Rin.Editor.Panes.Inspector;
using Serilog;

namespace Rin.Editor.Panes;

public class MyCustomView : View {
    readonly string selected = "none";

    // TODO: these states needs to be somehow persisted across renderings
    readonly State<string> selection1 = new();
    readonly State<string> selection2 = new();
    readonly State<bool> isToggled = new();
    readonly State<string> textField = new();
    readonly State<float> slider = new();
    readonly State<int> dragInt = new();

    // readonly TransformView transformView = new();

    readonly string[] test = { "foo", "bar", "strings" };

    Person[] people = { new("Foo", 42), new("Bar", 12), new("asdf", 69) };
    
    // @formatter:off
    protected override View Body =>
        VStack(
            Button(isToggled.Value ? "toggled" : selection2.Value, () => Log.Error("test"))
                .ContextMenu(
                    Button(selected),
                    MenuItem("Menu Item"),
                    MenuItem("Menu Item"),
                    Divider(),
                    Menu(
                        "Create",
                        MenuItem("Menu Item"),
                        MenuItem("Menu Item"),
                        MenuItem("Menu Item")
                    )
                ),
            Picker(selection1, "foo", "bar", "asdf"),
            Picker(selection2, "foo", "bar", "asdf"),
            Toggle("someToggle", isToggled),
            isToggled.Value ? Text("is checked!!") : Empty(),
            ForEach(test, Text),
            HStack(
                Toggle("Foo bar toggle", isToggled),
                TextField("Label", textField),
                // Picker("Picker", selection1, "foo", "asdf"),
                Toggle("Foo bar toggle", isToggled)
            ).Style(ToggleStyle.Button),
            
            Grid(
                GridRow(
                    Toggle("Toggle 1", isToggled),
                    TextField("Field 1", textField),
                    // Picker("Picker", selection1, "foo", "asdf"),
                    Toggle("Toggle 2", isToggled)
                ),
                GridRow(
                    Toggle("Toggle 3", isToggled),
                    Toggle("Toggle 4", isToggled)
                ),
                Divider(),
                Text("Random Text 1"),
                GridRow(
                    Toggle("Toggle 5", isToggled),
                    TextField(textField),
                    Picker("Picker 1", selection1, "foo", "asdf"),
                    Toggle("Toggle 6", isToggled)
                )
            ),
            
            Slider(slider),
            Drag(slider),
            Drag(dragInt),
            Table(people,
                    TableColumn($"Name {slider.Value}", (Person p) => p.Name),
                    TableColumn("Age", (Person p) => p.Age.ToString()),
                    TableColumn("Actions", (Person p) => Button($"Ban {p.Name}"))
                )
                .Style(new TableStyle { HasHeader = false }),
            
            CollapsingView("foo bar",
                VStack(
                    Text("Foo"),
                    Text("bar")
                )
            ),
            
            Button("outside")
            // transformView
        );
    // @formatter:on
    
    record Person(string Name, int Age);
}
