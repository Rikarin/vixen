using Arch.Core;
using Arch.Core.Utils;
using Arch.Relationships;
using ImGuiNET;
using Rin.Core.Components;
using Rin.Core.General;
using Rin.Core.UI;

namespace Rin.Editor.Panes;

sealed class HierarchyPane : Pane {
    readonly QueryDescription rootQuery = new QueryDescription().WithNone<Relationship<Parent>>();

    // TODO: move this to EditorManager
    public static Entity? Selected { get; set; }

    public HierarchyPane() : base("Hierarchy") { }

    void RenderEntity(Entity entity) {
        var world = SceneManager.ActiveScene.World;
        var name = $"[Entity {entity.Id}]";

        if (world.TryGet(entity, typeof(Name), out var nameObj)) {
            name = $"{((Name)nameObj).Value}##{entity.Id}";
        }

        if (entity.HasRelationship<Child>()) {
            var isOpened = ImGui.TreeNodeEx(
                name,
                (Selected == entity
                    ? ImGuiTreeNodeFlags.Selected
                    : ImGuiTreeNodeFlags.None)
                | ImGuiTreeNodeFlags.OpenOnArrow
                | ImGuiTreeNodeFlags.DefaultOpen
                | ImGuiTreeNodeFlags.SpanFullWidth
            );

            if (ImGui.IsItemClicked() || ImGui.IsItemFocused()) {
                Selected = entity;
            }

            ContextMenu(entity).Render();

            if (isOpened) {
                foreach (var child in entity.GetRelationships<Child>()) {
                    RenderEntity(child.Key);
                }

                ImGui.TreePop();
            }
        } else {
            if (ImGui.Selectable($"         {name}", Selected == entity, ImGuiSelectableFlags.SpanAllColumns)) {
                Selected = entity;
            }

            if (ImGui.IsItemFocused()) {
                Selected = entity;
            }

            ContextMenu(entity).Render();
        }
    }

    View ContextMenu(Entity? entity) {
        var view = new EmptyView();
        // @formatter:off
        view.ContextMenu(
            view.MenuItem("Create Empty", () => CreateEntity(entity, CreateEntityType.Empty)),
            view.MenuItem("Create GameObject", () => CreateEntity(entity, CreateEntityType.GameObject))
        );
        // @formatter:on

        return view;
    }

    void CreateEntity(Entity? parent, CreateEntityType type) {
        var types = type switch {
            CreateEntityType.Empty => Array.Empty<ComponentType>(),
            CreateEntityType.GameObject => new ComponentType[] { typeof(LocalToWorld), typeof(LocalTransform) },
            _ => Array.Empty<ComponentType>()
        };

        Application.InvokeOnMainThread(
            () => {
                var newEntity = SceneManager.ActiveScene.World.Create(types);
                Selected = newEntity;

                if (parent.HasValue) {
                    newEntity.AddRelationship<Parent>(parent.Value);
                }
            }
        );
    }

    protected override void OnRender() {
        var scene = SceneManager.ActiveScene;
        if (scene == null) {
            return;
        }

        if (ImGui.BeginChild("hierarchy")) {
            var isOpened = ImGui.TreeNodeEx(
                scene.Name,
                ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.SpanFullWidth
            );

            ContextMenu(null).Render();
            if (isOpened) {
                scene.World.Query(rootQuery, (in Entity entity) => RenderEntity(entity));
                ImGui.TreePop();
            }

            ImGui.EndChild();
        }
    }
}

enum CreateEntityType {
    Empty,
    GameObject
}
