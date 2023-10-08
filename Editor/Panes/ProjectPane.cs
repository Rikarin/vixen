using ImGuiNET;
using Rin.Core.UI;
using Serilog;

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
        myCustomView.Render();
        if (ImGui.BeginChild("project")) {
            ImGui.Text("foo bar");
            ImGui.EndChild();
        }

        ImGui.Columns(1);
    }
}

public class MyCustomView : View {
    readonly string selected = "none";
    // TODO: these states needs to be somehow persisted across renderings
    readonly State<string> selection1 = new();
    readonly State<string> selection2 = new();
    readonly State<bool> isToggled = new();

    string[] test = { "foo", "bar", "strings" };
    
    // @formatter:off
    protected override View Body =>
        VStack(
            Button(isToggled.Value ? "toggled" : selection2.Value, () => Log.Error("test"))
                .ContextMenu(
                    Button(selected),
                    MenuItem("Menu Item"),
                    MenuItem("Menu Item"),
                    Separator(),
                    Menu(
                        "Create",
                        MenuItem("Menu Item"),
                        MenuItem("Menu Item"),
                        MenuItem("Menu Item")
                    )
                ),
            Picker("first", selection1, "foo", "bar", "asdf"),
            Picker("##foobar", selection2, "foo", "bar", "asdf"),
            Toggle("someToggle", isToggled),
            isToggled.Value ? Text("is checked!!") : Empty(),
            ForEach(test, Text)
        );
    // @formatter:on
}
