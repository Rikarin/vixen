using Arch.Core;
using Arch.Core.Utils;
using Arch.Relationships;
using ImGuiNET;
using Rin.Core.Components;
using Rin.Core.General;
using Rin.Core.UI;
using Serilog;

namespace Rin.Editor.Panes;

sealed class HierarchyPane : Pane {
    // TODO: move this to EditorManager
    readonly QueryDescription rootQuery = new QueryDescription().WithNone<Relationship<Parent>>();
        
    public static Entity? Selected { get; set; }

    public HierarchyPane() : base("Hierarchy") { }

    void RenderEntity(Entity entity) {
        var world = SceneManager.ActiveScene.World;
        var name = $"Entity {entity.Id}";
        
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

            if (ImGui.IsItemClicked()) {
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
            if (ImGui.Selectable($"     {name}", Selected == entity, ImGuiSelectableFlags.SpanAllColumns)) {
                Selected = entity;
            }
            
            ContextMenu(entity).Render();
        }
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
        
        var newEntity = SceneManager.ActiveScene.World.Create(types);
        Log.Information("Debug: {Variable}", parent.HasValue);

        if (parent.HasValue) {
            newEntity.AddRelationship<Parent>(parent.Value);
        }
    }
}

enum CreateEntityType {
    Empty,
    GameObject
}