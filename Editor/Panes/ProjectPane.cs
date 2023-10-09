using ImGuiNET;
using Rin.Core.UI;
using Serilog;
using System.Numerics;

namespace Rin.Editor.Panes;

// TODO: iterating over all directories each frame is not the best way how to implement this
// Use File watcher instead
sealed class ProjectPane : Pane {
    string selectedPath = string.Empty;

    string search = string.Empty;

    readonly MyCustomView myCustomView = new();

    public ProjectPane() : base("Project") { }

    void RenderDictionary(string path) {
        foreach (var directory in Directory.GetDirectories(path)) {
            var openFlag = selectedPath == directory ? ImGuiTreeNodeFlags.Selected : ImGuiTreeNodeFlags.None;
            var isOpened = ImGui.TreeNodeEx(
                Path.GetFileName(directory),
                ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanFullWidth | openFlag
            );
            ContextMenuRender(directory);

            if (ImGui.IsItemClicked()) {
                selectedPath = directory;
            }

            if (isOpened) {
                RenderDictionary(directory);
                ImGui.TreePop();
            }
        }

        foreach (var file in Directory.GetFiles(path)) {
            if (Path.GetExtension(file) == ".meta") {
                continue;
            }

            var selected = file == selectedPath;
            var clicked = ImGui.Selectable(Path.GetFileName(file), ref selected, ImGuiSelectableFlags.SpanAllColumns);
            ContextMenuRender(file);

            if (clicked) {
                selectedPath = file;
            }
        }
    }

    void ContextMenuRender(string path) {
        var directory = File.Exists(path) ? Path.GetDirectoryName(path)! : path;

        if (ImGui.BeginPopupContextItem()) {
            ImGui.InputText("##Search", ref search, 128);
            ImGui.Separator();

            if (ImGui.BeginMenu("Create")) {
                if (ImGui.MenuItem("Folder")) {
                    Directory.CreateDirectory(Path.Combine(directory, "New Directory"));
                }

                ImGui.Separator();
                if (ImGui.MenuItem("C# Script")) {
                    // TODO
                }

                // TODO: finish other stuff
                ImGui.Separator();

                if (ImGui.MenuItem("Scene")) {
                    // TODO: create new scene
                }

                if (ImGui.MenuItem("Prefab")) {
                    // TODO
                }

                if (ImGui.MenuItem("Prefab Variant")) {
                    // TODO
                }

                if (ImGui.MenuItem("Material")) {
                    // TODO
                }

                if (ImGui.MenuItem("Material Variant")) {
                    // TODO
                }

                ImGui.EndMenu();
            }

            if (ImGui.MenuItem("Reveal in Finder")) {
                // TODO
            }

            if (ImGui.MenuItem("Delete")) { }

            ImGui.EndPopup();
        }
    }

    protected override void OnRender() {
        ImGui.Columns(2, "##ProjectColumns", false);
        if (ImGui.BeginChild("project")) {
            if (ImGui.TreeNodeEx("Project", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.SpanFullWidth)) {
                if (ImGui.BeginPopupContextItem()) {
                    ImGui.MenuItem("Asdf");
                    ImGui.EndPopup();
                }

                RenderDictionary(Gui.Project.RootDirectory);
                ImGui.TreePop();
            }

            ImGui.EndChild();
        }


        ImGui.NextColumn();
        ImGui.Text("lol");

        ViewContext.Reset();
        myCustomView.Render();
        if (ImGui.BeginChild("project")) {
            ImGui.Text("foo bar");
            ImGui.EndChild();
        }

        ImGui.Columns(1);

        ImGui.ShowStackToolWindow();
        float angle = 0;
        Vector3 vec3 = Vector3.Zero;
        ImGui.SliderFloat("foobar", ref angle, 0, 200);
        ImGui.SliderFloat3("asdf", ref vec3, 0, 200);
    }
}

public class MyCustomView : View {
    readonly string selected = "none";

    // TODO: these states needs to be somehow persisted across renderings
    readonly State<string> selection1 = new();
    readonly State<string> selection2 = new();
    readonly State<bool> isToggled = new();
    readonly State<string> textField = new();
    readonly State<float> slider = new();
    readonly State<int> dragInt = new();

    readonly TransformView transformView = new();

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
            
            // Grid(
            //     GridRow(
            //         Toggle("Toggle 1", isToggled),
            //         TextField("Field 1", textField),
            //         // Picker("Picker", selection1, "foo", "asdf"),
            //         Toggle("Toggle 2", isToggled)
            //     ),
            //     GridRow(
            //         Toggle("Toggle 3", isToggled),
            //         Toggle("Toggle 4", isToggled)
            //     ),
            //     Divider(),
            //     Text("Random Text 1"),
            //     GridRow(
            //         Toggle("Toggle 5", isToggled),
            //         TextField(textField),
            //         Picker("Picker 1", selection1, "foo", "asdf"),
            //         Toggle("Toggle 6", isToggled)
            //     )
            // ),
            //
            // Slider(slider),
            // Drag(slider),
            // Drag(dragInt),
            // Table(people,
            //     TableColumn($"Name {slider.Value}", (Person p) => p.Name),
            //     TableColumn("Age", (Person p) => p.Age.ToString()),
            //     TableColumn("Actions", (Person p) => Button($"Ban {p.Name}"))
            // )
            // .Style(new TableStyle { HasHeader = false }),
            //
            // CollapsingView("foo bar",
            //     VStack(
            //         Text("Foo"),
            //         Text("bar")
            //     )
            // ),
            //
            // Button("outside"),
            transformView
        );
    // @formatter:on
    
    record Person(string Name, int Age);
}