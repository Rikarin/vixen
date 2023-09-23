using ImGuiNET;

namespace Rin.Editor.Panes;

sealed class ProjectPane : Pane {
    string selectedPath = string.Empty;

    public ProjectPane() : base("Project") { }

    void RenderDictionary(string path) {
        foreach (var directory in Directory.GetDirectories(path)) {
            var treeNode = ImGui.TreeNode(Path.GetFileName(directory));

            if (ImGui.IsItemClicked(ImGuiMouseButton.Right)) {
                // TODO: show context menu
            }

            if (treeNode) {
                RenderDictionary(directory);
                ImGui.TreePop();
            }
        }

        foreach (var file in Directory.GetFiles(path)) {
            var selected = file == selectedPath;
            if (ImGui.Selectable(Path.GetFileName(file), ref selected)) {
                selectedPath = file;
            }
        }
    }

    protected override void OnRender() {
        if (ImGui.TreeNode("Project")) {
            RenderDictionary(Gui.Project.RootDirectory);
            ImGui.TreePop();
        }
    }
}
