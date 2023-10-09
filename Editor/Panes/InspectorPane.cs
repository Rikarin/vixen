using ImGuiNET;
using Rin.Core.General;
using Rin.Core.Math;
using System.Numerics;

namespace Rin.Editor.Panes;

sealed class InspectorPane : Pane {
    // Inspector params
    bool isEnabled;
    bool isStatic;
    string name = string.Empty;

    int tagId;
    int layerId;

    TransformView transformView = new();

    readonly Transform transform = new();
    Vector3 position;
    Vector3 rotation;
    Vector3 scale;

    public InspectorPane() : base("Inspector") {
        position = transform.LocalPosition;
        rotation = transform.LocalRotation.ToEulerAngles();
        scale = transform.LocalScale;
    }

    void RenderTransform() {
        if (ImGui.CollapsingHeader("Transform", ImGuiTreeNodeFlags.DefaultOpen)) {
            if (ImGui.BeginTable("TransformTable", 4)) {
                // Position
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text("Position");

                ImGui.TableNextColumn();
                if (ImGui.DragFloat("X##PositionX", ref position.X, 0.03f, 0, 0, "%.2f")) {
                    transform.LocalPosition = position;
                }

                ImGui.TableNextColumn();
                if (ImGui.DragFloat("Y##PositionY", ref position.Y, 0.03f, 0, 0, "%.2f")) {
                    transform.LocalPosition = position;
                }

                ImGui.TableNextColumn();
                if (ImGui.DragFloat("Z##PositionZ", ref position.Z, 0.03f, 0, 0, "%.2f")) {
                    transform.LocalPosition = position;
                }

                // Rotation
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text("Rotation");

                ImGui.TableNextColumn();
                if (ImGui.DragFloat("X##RotationX", ref rotation.X, 0.03f, 0, 0, "%.2f")) {
                    transform.LocalRotation = rotation.ToQuaternion();
                }

                ImGui.TableNextColumn();
                if (ImGui.DragFloat("Y##RotationY", ref rotation.Y, 0.03f, 0, 0, "%.2f")) {
                    transform.LocalRotation = rotation.ToQuaternion();
                }

                ImGui.TableNextColumn();
                if (ImGui.DragFloat("Z##RotationZ", ref rotation.Z, 0.03f, 0, 0, "%.2f")) {
                    transform.LocalRotation = rotation.ToQuaternion();
                }

                // Scale
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text("Scale");

                ImGui.TableNextColumn();
                if (ImGui.DragFloat("X##ScaleX", ref scale.X, 0.03f, 0, 0, "%.2f")) {
                    transform.LocalScale = scale;
                }

                ImGui.TableNextColumn();
                if (ImGui.DragFloat("Y##ScaleY", ref scale.Y, 0.03f, 0, 0, "%.2f")) {
                    transform.LocalScale = scale;
                }

                ImGui.TableNextColumn();
                if (ImGui.DragFloat("Z##ScaleZ", ref scale.Z, 0.03f, 0, 0, "%.2f")) {
                    transform.LocalScale = scale;
                }

                ImGui.EndTable();
            }
        }
    }

    protected override void OnRender() {
        ImGui.Checkbox("##IsEnabled", ref isEnabled);
        ImGui.SameLine();
        ImGui.InputText("##Name", ref name, 64);
        ImGui.SameLine();
        ImGui.Checkbox("Static", ref isStatic);

        ImGui.Columns(2, "##LayerAndTag", false);
        var tags = Gui.Project.Tags;
        if (ImGui.BeginCombo("Tag", tags[tagId])) {
            for (var i = 0; i < tags.Count; i++) {
                if (ImGui.Selectable(tags[i], tagId == i)) {
                    tagId = i;
                }
            }

            ImGui.Separator();
            if (ImGui.Selectable("Add Tag")) {
                Gui.OpenPane<TagPane>();
            }

            ImGui.EndCombo();
        }

        ImGui.NextColumn();
        var layers = Gui.Project.Layers;
        if (ImGui.BeginCombo("Layer", layers[layerId])) {
            for (var i = 0; i < layers.Count; i++) {
                if (ImGui.Selectable(layers[i], layerId == i)) {
                    layerId = i;
                }
            }

            ImGui.Separator();
            if (ImGui.Selectable("Add Layer")) {
                // TODO: open layer window
            }

            ImGui.EndCombo();
        }

        ImGui.Columns(1);
        ImGui.Spacing();
        ImGui.Spacing();

        RenderTransform();
        transformView.Render();


        if (ImGui.CollapsingHeader("Mesh Filter")) {
            ImGui.Text("Foo");
            ImGui.Text("Foo");
        }
    }
}
