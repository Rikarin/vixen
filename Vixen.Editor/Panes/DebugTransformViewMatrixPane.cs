namespace Vixen.Editor.Panes;

sealed class DebugTransformViewMatrixPane : Pane {
    public DebugTransformViewMatrixPane() : base("Debug Transform View Matrix") { }

    protected override void OnRender() {
        // var vm = transform.ViewMatrix;
        // var line1 = new Vector4(vm.M11, vm.M12, vm.M13, vm.M14);
        // var line2 = new Vector4(vm.M21, vm.M22, vm.M23, vm.M24);
        // var line3 = new Vector4(vm.M31, vm.M32, vm.M33, vm.M34);
        // var line4 = new Vector4(vm.M41, vm.M42, vm.M43, vm.M44);
        // ImGui.InputFloat4("Line 1", ref line1);
        // ImGui.InputFloat4("Line 2", ref line2);
        // ImGui.InputFloat4("Line 3", ref line3);
        // ImGui.InputFloat4("Line 4", ref line4); 
    }
}
