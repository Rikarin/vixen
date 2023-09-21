using ImGuiNET;
using Rin.Core.General;
using Rin.Core.Math;
using System.Drawing;
using System.Numerics;

namespace Rin.Editor.Elements;

public static class Inspector {
    public static bool IsOpened = true;
    public static bool IsOpenedDebug;

    static bool isEnabled;
    static bool isStatic;
    static string name = string.Empty;

    static readonly string[] layers = { "Foo Layer", "bar layer" };
    static readonly string[] tags = { "Foo Tag", "bar Tag", "asdf Tag" };
    static int tagId;
    static int layerId;

    static readonly Transform transform = new();
    static Vector3 position = transform.LocalPosition;
    static Vector3 rotation = transform.LocalRotation.ToEulerAngles();
    static Vector3 scale = transform.LocalScale;

    public static void Render() {
        if (IsOpenedDebug) {
            RenderDebugTransformViewMatrix();
        }
        
        if (!IsOpened) {
            return;
        }

        ImGui.Begin("Inspector", ref IsOpened);
        ImGui.Checkbox("##IsEnabled", ref isEnabled);
        ImGui.SameLine();
        ImGui.InputText("##Name", ref name, 64);
        ImGui.SameLine();
        ImGui.Checkbox("Static", ref isStatic);

        ImGui.Columns(2, "##LayerAndTag", false);
        if (ImGui.BeginCombo("Tag", tags[tagId])) {
            for (var i = 0; i < tags.Length; i++) {
                if (ImGui.Selectable(tags[i], tagId == i)) {
                    tagId = i;
                }
            }

            ImGui.EndCombo();
        }

        ImGui.NextColumn();
        if (ImGui.BeginCombo("Layer", layers[layerId])) {
            for (var i = 0; i < layers.Length; i++) {
                if (ImGui.Selectable(layers[i], layerId == i)) {
                    layerId = i;
                }
            }

            ImGui.EndCombo();
        }

        ImGui.Columns(1);
        ImGui.Spacing();
        ImGui.Spacing();

        RenderTransform();


        if (ImGui.CollapsingHeader("Mesh Filter")) {
            ImGui.Text("Foo");
        }

        ImGui.End();
    }

    static void RenderTransform() {
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

    static void RenderDebugTransformViewMatrix() {
        ImGui.Begin("Debug Transform View Matrix");
        var vm = transform.ViewMatrix;
        var line1 = new Vector4(vm.M11, vm.M12, vm.M13, vm.M14);
        var line2 = new Vector4(vm.M21, vm.M22, vm.M23, vm.M24);
        var line3 = new Vector4(vm.M31, vm.M32, vm.M33, vm.M34);
        var line4 = new Vector4(vm.M41, vm.M42, vm.M43, vm.M44);
        ImGui.InputFloat4("Line 1", ref line1);
        ImGui.InputFloat4("Line 2", ref line2);
        ImGui.InputFloat4("Line 3", ref line3);
        ImGui.InputFloat4("Line 4", ref line4);

        ImGui.End();
    }
}

// ImGui.PushStyleColor(ImGuiCol.FrameBg, Color.Red.ToVector4());
// ImGui.PopStyleColor();
static class ColorExtensions {
    public static Vector4 ToVector4(this Color color) =>
        new(color.R / 256f, color.G / 256f, color.B / 256f, color.A / 256f);
}
