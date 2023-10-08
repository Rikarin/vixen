using ImGuiNET;
using Rin.Core.General;
using Rin.Editor.Panes;
using Rin.Rendering;
using Serilog;
using System.Drawing;
using System.Numerics;

namespace Rin.Editor;

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

        // var mainWindow = this.application.MainWindow;
        // mainWindow.Load += OnStart;
        // mainWindow.Closing += OnClosing;
        // mainWindow.Render += OnRender;

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
    }

    public void Dispose() {
        var mainWindow = application.MainWindow;
        // mainWindow.Load -= OnStart;
        mainWindow.Closing -= OnClosing;
        // mainWindow.Render -= OnRender;
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

        Log.Information("font init");

        // ImGui.StyleColorsDark();
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

        // tODO: testing only
        SetTheme();

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

        // Start pane renderings
        foreach (var pane in panes.Values) {
            pane.Render();
        }

        if (firstTime) {
            firstTime = false;
            ImGui.LoadIniSettingsFromDisk(DefaultPath);
        }

        if (ImGui.BeginMainMenuBar()) {
            // if (ImGui.BeginMenu("File")) {
            //     if (ImGui.MenuItem("New", "Ctrl+N")) {
            //         opened = true;
            //     }
            //     ImGui.EndMenu();
            // }

            if (ImGui.BeginMenu("File")) {
                if (ImGui.MenuItem("Load Layout")) {
                    ImGui.LoadIniSettingsFromDisk("imgui_layout.ini");
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Edit")) {
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Assets")) {
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Game Object")) {
                if (ImGui.MenuItem("Create Empty")) {
                    // TODO: create empty object in the current scene
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Component")) {
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Window")) {
                if (ImGui.BeginMenu("Layouts")) {
                    if (ImGui.MenuItem("Basic")) {
                        ImGui.LoadIniSettingsFromDisk(Path.Combine(settingsPath, "Layouts", "Basic.ini"));
                    }

                    ImGui.EndMenu();
                }

                ImGui.Separator();
                if (ImGui.BeginMenu("General")) {
                    if (ImGui.MenuItem("Scene")) {
                        OpenPane<ScenePane>();
                    }

                    if (ImGui.MenuItem("Game")) {
                        OpenPane<GamePane>();
                    }

                    if (ImGui.MenuItem("Inspector", "Ctrl+I")) {
                        OpenPane<InspectorPane>();
                    }

                    if (ImGui.MenuItem("Hierarchy")) {
                        OpenPane<HierarchyPane>();
                    }

                    if (ImGui.MenuItem("Project")) {
                        OpenPane<ProjectPane>();
                    }

                    if (ImGui.MenuItem("Console")) {
                        OpenPane<ConsolePane>();
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Debug")) {
                    if (ImGui.MenuItem("Debug Transform View Matrix")) {
                        OpenPane<DebugTransformViewMatrixPane>();
                    }

                    if (ImGui.MenuItem("Stats")) {
                        OpenPane<StatsPane>();
                    }

                    ImGui.EndMenu();
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Help")) {
                if (ImGui.MenuItem("Demo Window")) { }

                ImGui.EndMenu();
            }

            ImGui.EndMainMenuBar();
        }

        // ImGui.ShowDemoWindow();
        ImGui.End();
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
	    // colors[(int)ImGuiCol.HeaderHovered] = Color.FromArgb(255, 48, 48, 48).ToVector4();
	    // colors[(int)ImGuiCol.HeaderActive] = Color.FromArgb(255, 48, 48, 48).ToVector4();
	    
	    // Frame Background (Inputs, Selects, Charts)
	    colors[(int)ImGuiCol.FrameBg] = MonoColor(17);
	    colors[(int)ImGuiCol.FrameBgHovered] = MonoColor(26);
	    colors[(int)ImGuiCol.FrameBgActive] = MonoColor(31);
	    
	    // Tabs
	    colors[(int)ImGuiCol.Tab] = MonoColor(21);
	    colors[(int)ImGuiCol.TabActive] = MonoColor(41);
	    colors[(int)ImGuiCol.TabHovered] = MonoColor(45);
	    
	    colors[(int)ImGuiCol.TabUnfocused] = MonoColor(21);
	    colors[(int)ImGuiCol.TabUnfocusedActive] = MonoColor(41);
	    
	    // Title
	    // colors[(int)ImGuiCol.WindowBg] = Color.FromArgb(255, 21, 21, 21).ToVector4();
	    colors[(int)ImGuiCol.WindowBg] = Color.Red.ToVector4();
	    colors[(int)ImGuiCol.TitleBg] = MonoColor(21);
	    colors[(int)ImGuiCol.TitleBgActive] = MonoColor(21);
	    // colors[ImGuiCol_TitleBg]			= ImGui::ColorConvertU32ToFloat4(Colors::Theme::titlebar);
	    // colors[ImGuiCol_TitleBgActive]		= ImGui::ColorConvertU32ToFloat4(Colors::Theme::titlebar);
	    // colors[ImGuiCol_TitleBgCollapsed]	= ImVec4{ 0.15f, 0.1505f, 0.151f, 1.0f };
	    //
	    // Resize Grip
	    // colors[ImGuiCol_ResizeGrip]			= ImVec4(0.91f, 0.91f, 0.91f, 0.25f);
	    // colors[ImGuiCol_ResizeGripHovered]	= ImVec4(0.81f, 0.81f, 0.81f, 0.67f);
	    // colors[ImGuiCol_ResizeGripActive]	= ImVec4(0.46f, 0.46f, 0.46f, 0.95f);
	    //
	    // Scrollbar
	    // colors[ImGuiCol_ScrollbarBg]		= ImVec4(0.02f, 0.02f, 0.02f, 0.53f);
	    // colors[ImGuiCol_ScrollbarGrab]		= ImVec4(0.31f, 0.31f, 0.31f, 1.0f);
	    // colors[ImGuiCol_ScrollbarGrabHovered] = ImVec4(0.41f, 0.41f, 0.41f, 1.0f);
	    // colors[ImGuiCol_ScrollbarGrabActive] = ImVec4(0.51f, 0.51f, 0.51f, 1.0f);
	    
	    // Check Mark
	    colors[(int)ImGuiCol.CheckMark] = Color.FromArgb(255, 23, 90, 193).ToVector4();
	    
	    // Slider
	    // Xcode color
	    colors[(int)ImGuiCol.SliderGrab] = Color.FromArgb(255, 23, 90, 193).ToVector4();
	    colors[(int)ImGuiCol.SliderGrabActive] = MonoColor(168);
	    
	    // Text
	    colors[(int)ImGuiCol.Text] = MonoColor(200);
	    
	    // Separator
	    // colors[ImGuiCol_Separator]			= ImGui::ColorConvertU32ToFloat4(Colors::Theme::backgroundDark);
	    // colors[ImGuiCol_SeparatorActive]	= ImGui::ColorConvertU32ToFloat4(Colors::Theme::highlight);
	    // colors[ImGuiCol_SeparatorHovered]	= ImColor(39, 185, 242, 150);
	    
	    // Window Background
	    colors[(int)ImGuiCol.WindowBg] = MonoColor(21);

	    // colors[ImGuiCol_ChildBg]			= ImGui::ColorConvertU32ToFloat4(Colors::Theme::background);
	    // colors[ImGuiCol_PopupBg]			= ImGui::ColorConvertU32ToFloat4(Colors::Theme::backgroundPopup);
	    // colors[ImGuiCol_Border]				= ImGui::ColorConvertU32ToFloat4(Colors::Theme::backgroundDark);
	    
	    // Tables
	    // colors[ImGuiCol_TableHeaderBg]		= ImGui::ColorConvertU32ToFloat4(Colors::Theme::groupHeader);
	    // colors[ImGuiCol_TableBorderLight]	= ImGui::ColorConvertU32ToFloat4(Colors::Theme::backgroundDark);
	    
	    // Menubar
	    // colors[ImGuiCol_MenuBarBg]			= ImVec4{ 0.0f, 0.0f, 0.0f, 0.0f };
    }

    Vector4 MonoColor(int value) => Color.FromArgb(255, value, value, value).ToVector4();
}
