using Vixen.Core;
using Vixen.Core.General;
using Vixen.UI;

namespace Vixen.Editor.Panes;

sealed class EditorCameraDebugPane : Pane {
    public EditorCameraDebugPane() : base("Editor Camera Debug") { }

    protected override void OnRender() {
        List<(string, string)> data = new();
        EditorCamera camera = SceneManager.ActiveScene.World.GetSingleton<EditorCamera>();
        
        data.Add(("Mode", camera.Mode.ToString()));
        data.Add(("Focal Point", camera.FocalPoint.ToString()));
        data.Add(("Direction", camera.Direction.ToString()));
        data.Add(("Distance", camera.Distance.ToString()));
        data.Add(("Vertical FOV", camera.VerticalFov.ToString()));
        data.Add(("Near Clip", camera.FarClip.ToString()));
        data.Add(("Far Clip", camera.NearClip.ToString()));
        data.Add(("Zoom Speed", camera.ZoomSpeed.ToString()));
        
        new EditorCameraView(data.ToArray()).Render();
    }
}


public class EditorCameraView : View {
    readonly (string, string)[] values;

    public EditorCameraView((string, string)[] values) {
        this.values = values;
    }
    
    // @formatter:off
    protected override View Body =>
        VStack(
            Table(values.ToArray(),
                TableColumn<(string, string)>("Name", value => Text(value.Item1)),
                TableColumn<(string, string)>("Value", value => Text(value.Item2))
            )
        );
    // formatter:on
}