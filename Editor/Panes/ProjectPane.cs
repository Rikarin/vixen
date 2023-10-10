using ImGuiNET;
using Rin.Core.UI;

namespace Rin.Editor.Panes;

// TODO: iterating over all directories each frame is not the best way how to implement this
// Use File watcher instead
sealed class ProjectPane : Pane {
    string selectedPath = string.Empty;
    readonly State<string> search = new();

    public ProjectPane() : base("Project") { }

    void RenderDictionary(string path) {
        foreach (var directory in Directory.GetDirectories(path)) {
            var openFlag = selectedPath == directory ? ImGuiTreeNodeFlags.Selected : ImGuiTreeNodeFlags.None;
            var isOpened = ImGui.TreeNodeEx(
                Path.GetFileName(directory),
                ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanFullWidth | openFlag
            );
            ContextMenuView(new EmptyView(), path).Render();

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
            ContextMenuView(new EmptyView(), path).Render();

            if (clicked) {
                selectedPath = file;
            }
        }
    }

    View ContextMenuView(View view, string path) {
        var directory = File.Exists(path) ? Path.GetDirectoryName(path)! : path;
        
        // @formatter:off
        view.ContextMenu(
            view.TextField(search),
            view.Divider(),
            view.Menu("Create",
                view.MenuItem("Folder", () => {
                    Directory.CreateDirectory(Path.Combine(directory, "New Directory"));
                }),
                
                view.Divider(),
                view.MenuItem("C# Script"),
                
                view.Divider(),
                view.MenuItem("Scene"),
                view.MenuItem("Prefab"),
                view.MenuItem("Prefab Variant"),
                view.MenuItem("Material"),
                view.MenuItem("Material Variant")
            ),
            view.MenuItem("Open in Finder"),
            view.MenuItem("Delete")
        );
        // @formatter:on

        return view;
    }

    protected override void OnRender() {
        ImGui.Columns(2, "##ProjectColumns", false);
        if (ImGui.BeginChild("project")) {
            if (ImGui.TreeNodeEx("Project", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.SpanFullWidth | ImGuiTreeNodeFlags.SpanFullWidth)) {
                RenderDictionary(Gui.Project.RootDirectory);
                ImGui.TreePop();
            }

            ImGui.EndChild();
        }

        ImGui.NextColumn();
        ImGui.Columns(1);
    }
}