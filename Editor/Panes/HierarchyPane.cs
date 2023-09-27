using ImGuiNET;

namespace Rin.Editor.Panes;

sealed class HierarchyPane : Pane {
    bool foobar;

    public HierarchyPane() : base("Hierarchy") { }

    protected override void OnRender() {
        if (ImGui.TreeNodeEx("Scene Name", ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.DefaultOpen)) {
            // TODO: render scene objects

            var opened = ImGui.TreeNodeEx(
                "Basdffsdf",
                ImGuiTreeNodeFlags.OpenOnArrow | (foobar ? ImGuiTreeNodeFlags.Selected : ImGuiTreeNodeFlags.None)
            );

            if (ImGui.IsItemClicked()) {
                foobar = !foobar;
            }

            if (opened) {
                ImGui.Selectable("Foo");

                if (ImGui.TreeNode("omg lol")) {
                    ImGui.Text("asdfsdaff");
                    ImGui.TreePop();
                }

                ImGui.TreePop();
            }

            ImGui.TreePop();
        }
    }
}