using ImGuiNET;
using Serilog;

namespace Rin.Editor.Panes;

// TODO: iterating over all directories each frame is not the best way how to implement this
// Use File watcher instead
sealed class ProjectPane : Pane {
    string selectedPath = string.Empty;

    public ProjectPane() : base("Project") { }

    void RenderDictionary(string path) {
        foreach (var directory in Directory.GetDirectories(path)) {
            var openFlag = selectedPath == directory ? ImGuiTreeNodeFlags.Selected : ImGuiTreeNodeFlags.None;
            var isOpened = ImGui.TreeNodeEx(Path.GetFileName(directory), ImGuiTreeNodeFlags.OpenOnArrow | openFlag);
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
            var clicked = ImGui.Selectable(Path.GetFileName(file), ref selected);
            ContextMenuRender(file);
            
            if (clicked) {
                selectedPath = file;
            }
        }
    }

    protected override void OnRender() {
        if (ImGui.TreeNodeEx("Project", ImGuiTreeNodeFlags.DefaultOpen)) {
            if (ImGui.BeginPopupContextItem()) {
                ImGui.MenuItem("Asdf");
                ImGui.EndPopup();
            }

            RenderDictionary(Gui.Project.RootDirectory);
            ImGui.TreePop();
        }
    }

    string search = string.Empty;
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

            if (ImGui.MenuItem("Delete")) {
                
            }
            
            ImGui.EndPopup();
        }
    }
}
