using ImGuiNET;
using Serilog;
using System.Drawing;
using System.Numerics;
using Vixen.Core.General;
using Vixen.Editor.Panes;
using Vixen.Editor.Panes.Inspector;
using Vixen.UI;

namespace Vixen.Editor;

sealed class GuiRenderer : IDisposable {
    readonly Application application;
    readonly Dictionary<Type, Pane> panes = new();
    readonly bool fullScreenDock = true;
    readonly string settingsPath;
    bool firstTime = true;
    bool dockSpaceOpen = true;

    public Project Project { get; private set; }

    string DefaultPath => Path.Combine(settingsPath, "Default.ini");

    public GuiRenderer(Application application, Project project) {
        this.application = application;
        Project = project;
        settingsPath = Path.Combine(project.RootDirectory, "ProjectSettings", "Editor");
        Directory.CreateDirectory(settingsPath);

        var mainWindow = this.application.MainWindow;
        mainWindow.Closing += OnClosing;

        AddPane<StatsPane>();
        AddPane<HierarchyPane>();
        AddPane<ProjectPane>();
        AddPane<ConsolePane>();
        AddPane<LightningPane>();
        AddPane<InspectorPane>();
        AddPane<ScenePane>();
        AddPane<DebugTransformViewMatrixPane>();
        AddPane<TagPane>();
        AddPane<GamePane>();

        AddPane<ProfilerPane>();
        AddPane<EditorCameraDebugPane>();

        // TODO: Load/Save of opened windows

        // This is for testing purpose only; till load/save will be implemented
        OpenPane<HierarchyPane>();
        OpenPane<InspectorPane>();
        OpenPane<ProjectPane>();
        OpenPane<ConsolePane>();
        OpenPane<LightningPane>();
        OpenPane<ScenePane>();
        OpenPane<GamePane>();

        OpenPane<ProfilerPane>();
        OpenPane<EditorCameraDebugPane>();
    }

    public void Dispose() {
        var mainWindow = application.MainWindow;
        mainWindow.Closing -= OnClosing;
    }

    public void OpenPane<T>() => panes[typeof(T)].Open();

    public void OnRender(float deltaTime) {
        OnUpdate();

        ImGui.UpdatePlatformWindows();
        ImGui.RenderPlatformWindowsDefault();
    }

    public void OnStart() {
        var io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable
            | ImGuiConfigFlags.ViewportsEnable
            | ImGuiConfigFlags.NavEnableKeyboard;

        SetTheme();

        // io.BackendFlags |=
        //     ImGuiBackendFlags.HasMouseCursors
        //     | ImGuiBackendFlags.HasSetMousePos
        //     // | ImGuiBackendFlags.RendererHasViewports
        // | ImGuiBackendFlags.PlatformHasViewports;

        Log.Information("GUI Renderer Started");
    }

    public void OnUpdate() {
        const ImGuiDockNodeFlags dockNodeFlags = ImGuiDockNodeFlags.None | ImGuiDockNodeFlags.PassthruCentralNode;

        // We are using the ImGuiWindowFlags_NoDocking flag to make the parent window not dockable into,
        // because it would be confusing to have two docking targets within each others.
        var windowFlags = ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoDocking;
        if (fullScreenDock) {
            var viewport = ImGui.GetMainViewport();
            ImGui.SetNextWindowPos(viewport.Pos);
            ImGui.SetNextWindowSize(viewport.Size);
            ImGui.SetNextWindowViewport(viewport.ID);

            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);

            windowFlags |= ImGuiWindowFlags.NoTitleBar
                | ImGuiWindowFlags.NoCollapse
                | ImGuiWindowFlags.NoResize
                | ImGuiWindowFlags.NoMove
                | ImGuiWindowFlags.NoBringToFrontOnFocus
                | ImGuiWindowFlags.NoNavFocus;
        }

        if (dockNodeFlags.HasFlag(ImGuiDockNodeFlags.PassthruCentralNode)) {
            windowFlags |= ImGuiWindowFlags.NoBackground;
        }

        // Important: note that we proceed even if Begin() returns false (aka window is collapsed).
        // This is because we want to keep our DockSpace() active. If a DockSpace() is inactive, 
        // all active windows docked into it will lose their parent and become undocked.
        // We cannot preserve the docking relationship between an active window and an inactive docking, otherwise 
        // any change of dockspace/settings would lead to windows being stuck in limbo and never being visible.
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        ImGui.Begin("Rin Editor", ref dockSpaceOpen, windowFlags);
        ImGui.PopStyleVar();

        if (fullScreenDock) {
            ImGui.PopStyleVar(2);
        }

        var io = ImGui.GetIO();
        var style = ImGui.GetStyle();
        var minWindowSizeX = style.WindowMinSize.X;
        style.WindowMinSize.X = 370;

        if (io.ConfigFlags.HasFlag(ImGuiConfigFlags.DockingEnable)) {
            var dockSpaceId = ImGui.GetID("DockSpace");
            ImGui.DockSpace(dockSpaceId, Vector2.Zero, dockNodeFlags);
        }

        style.WindowMinSize.X = minWindowSizeX;
        io.WantSaveIniSettings = false; // Doesn't work or what

        // Reset ViewContext
        ViewContext.Reset();

        // Start pane renderings
        foreach (var pane in panes.Values) {
            pane.Render();
        }

        if (firstTime) {
            firstTime = false;
            ImGui.LoadIniSettingsFromDisk(DefaultPath);
        }

        if (ImGui.BeginMainMenuBar()) {
            MenuView().Render();
            ImGui.EndMainMenuBar();
        }

        // ImGui.ShowDemoWindow();
        ImGui.End();
    }

    View MenuView() {
        var x = new EmptyView();
        
        // @formatter:off
        return x.VStack(
            x.Menu("File",
                x.MenuItem("New"),
                x.MenuItem("Load Layout", () => ImGui.LoadIniSettingsFromDisk("imgui_layout.ini"))
            ),
            x.Menu("Edit"),
            x.Menu("Assets"),
            x.Menu("Game Object",
                x.MenuItem("Create Empty")
            ),
            x.Menu("Component"),
            x.Menu("Window",
	            x.Menu("Layouts",
					x.MenuItem("Basic", () =>ImGui.LoadIniSettingsFromDisk(Path.Combine(settingsPath, "Layouts", "Basic.ini")))
				),
                x.Divider(),
	            x.Menu("General",
					x.MenuItem("Scene", OpenPane<ScenePane>),
					x.MenuItem("Game", OpenPane<GamePane>),
					x.MenuItem("Inspector", OpenPane<InspectorPane>),
					x.MenuItem("Hierarchy", OpenPane<HierarchyPane>),
					x.MenuItem("Project", OpenPane<ProjectPane>),
					x.MenuItem("Console", OpenPane<ConsolePane>)
                ),
                x.Menu("Debug",
					x.MenuItem("Debug Transform View Matrix", OpenPane<DebugTransformViewMatrixPane>),
					x.MenuItem("Stats", OpenPane<StatsPane>),
					x.MenuItem("Profiler", OpenPane<ProfilerPane>)
                )
			),
            x.Menu("Help",
                x.MenuItem("Demo Window")
            )
        );
        // @formatter:on
    }

    void OnClosing() {
        ImGui.SaveIniSettingsToDisk(DefaultPath);
    }

    void AddPane<T>() where T : Pane, new() {
        panes.Add(typeof(T), new T { Gui = this });
    }

    void SetTheme() {
        // Style
        var style = ImGui.GetStyle();

        style.FrameRounding = 2.5f;
        style.FrameBorderSize = 1.0f;
        // style.FrameRounding = 0;
        // style.FrameBorderSize = 0;
        style.IndentSpacing = 11.0f;

        // Colors
        var colors = ImGui.GetStyle().Colors;

        // Buttons
        colors[(int)ImGuiCol.Button] = MonoColor(56);
        colors[(int)ImGuiCol.ButtonHovered] = MonoColor(70);
        colors[(int)ImGuiCol.ButtonActive] = MonoColor(56);

        // Headers - Group tabs
        colors[(int)ImGuiCol.Header] = MonoColor(48);
        colors[(int)ImGuiCol.HeaderHovered] = MonoColor(60);
        colors[(int)ImGuiCol.HeaderActive] = MonoColor(75);

        // Frame Background (Inputs, Selects, Charts)
        colors[(int)ImGuiCol.FrameBg] = MonoColor(17);
        colors[(int)ImGuiCol.FrameBgHovered] = MonoColor(26);
        colors[(int)ImGuiCol.FrameBgActive] = MonoColor(31);

        // Tabs
        colors[(int)ImGuiCol.Tab] = MonoColor(21);
        colors[(int)ImGuiCol.TabActive] = MonoColor(36);
        colors[(int)ImGuiCol.TabHovered] = MonoColor(45);

        colors[(int)ImGuiCol.TabUnfocused] = MonoColor(21);
        colors[(int)ImGuiCol.TabUnfocusedActive] = MonoColor(41);

        // Title
        colors[(int)ImGuiCol.TitleBg] = MonoColor(21);
        colors[(int)ImGuiCol.TitleBgActive] = MonoColor(21);
        colors[(int)ImGuiCol.TitleBgCollapsed] = MonoColor(21);

        // Resize Grip
        colors[(int)ImGuiCol.ResizeGrip] = MonoColor(55);
        colors[(int)ImGuiCol.ResizeGripHovered] = MonoColor(65);
        colors[(int)ImGuiCol.ResizeGripActive] = MonoColor(75);

        // Scrollbar
        colors[(int)ImGuiCol.ScrollbarBg] = MonoColor(17);
        colors[(int)ImGuiCol.ScrollbarGrab] = MonoColor(87);
        colors[(int)ImGuiCol.ScrollbarGrabHovered] = MonoColor(97);
        colors[(int)ImGuiCol.ScrollbarGrabActive] = MonoColor(107);

        // Check Mark
        colors[(int)ImGuiCol.CheckMark] = Color.FromArgb(255, 23, 90, 193).ToVector4();

        // Slider
        // Xcode color
        colors[(int)ImGuiCol.SliderGrab] = Color.FromArgb(255, 23, 90, 193).ToVector4();
        colors[(int)ImGuiCol.SliderGrabActive] = MonoColor(168);

        // Text
        colors[(int)ImGuiCol.Text] = MonoColor(200);

        // Separator
        colors[(int)ImGuiCol.Separator] = MonoColor(48);
        colors[(int)ImGuiCol.SeparatorHovered] = MonoColor(65);
        colors[(int)ImGuiCol.SeparatorActive] = MonoColor(75);

        // Window Background
        colors[(int)ImGuiCol.WindowBg] = MonoColor(36);
        colors[(int)ImGuiCol.ChildBg] = MonoColor(26);
        // colors[(int)ImGuiCol.PopupBg] = MonoColor(21);
        colors[(int)ImGuiCol.Border] = MonoColor(8);

        // Tables
        // colors[ImGuiCol_TableHeaderBg]		= ImGui::ColorConvertU32ToFloat4(Colors::Theme::groupHeader);
        // colors[ImGuiCol_TableBorderLight]	= ImGui::ColorConvertU32ToFloat4(Colors::Theme::backgroundDark);

        // Menubar
        colors[(int)ImGuiCol.MenuBarBg] = MonoColor(21);
    }

    Vector4 MonoColor(int value) => Color.FromArgb(255, value, value, value).ToVector4();
}
