using ImGuiNET;
using Rin.Core.General;
using Rin.Editor.Elements;
using Rin.Platform.Silk;
using Serilog;
using Silk.NET.OpenGL.Extensions.ImGui;
using System.Numerics;

namespace Rin.Editor;

public class ImGuiRenderer : IDisposable {
    ImGuiController controller;
    readonly Application application;

    // Platform_CreateWindow _createWindow;

    public ImGuiRenderer(Application application) {
        this.application = application;
        application.Load += OnStart;
        application.Render += OnRender;
    }

    public void Dispose() {
        application.Load -= OnStart;
        application.Render -= OnRender;
    }

    void OnRender(float deltaTime) {
        OnUpdate(deltaTime);

        controller.Render();
        ImGui.UpdatePlatformWindows();
        ImGui.RenderPlatformWindowsDefault();
    }

    protected virtual void OnStart() {
        // This is terrifying
        var silkWindow = (SilkWindow)Application.Window.handle;
        controller = new(silkWindow.Gl, silkWindow.silkWindow, silkWindow.input);

        // Multi-Viewport
        // var platformIo = ImGui.GetPlatformIO();
        // _createWindow = _CreateWindow;
        // platformIo.Platform_CreateWindow = Marshal.GetFunctionPointerForDelegate(_createWindow);
        // platformIo.Platform_DestroyWindow = Marshal.GetFunctionPointerForDelegate(() => Log.Information("Called"));
        // platformIo.Platform_DestroyWindow = Marshal.GetFunctionPointerForDelegate(() => Log.Information("Called"));
        // platformIo.Platform_GetWindowPos = Marshal.GetFunctionPointerForDelegate(() => Log.Information("Called"));
        // platformIo.Platform_ShowWindow = Marshal.GetFunctionPointerForDelegate(() => Log.Information("Called"));
        // platformIo.Platform_SetWindowPos = Marshal.GetFunctionPointerForDelegate(() => Log.Information("Called"));
        // platformIo.Platform_SetWindowSize = Marshal.GetFunctionPointerForDelegate(() => Log.Information("Called"));
        // platformIo.Platform_GetWindowSize = Marshal.GetFunctionPointerForDelegate(() => Log.Information("Called"));
        // platformIo.Platform_SetWindowFocus = Marshal.GetFunctionPointerForDelegate(() => Log.Information("Called"));
        // platformIo.Platform_GetWindowFocus = Marshal.GetFunctionPointerForDelegate(() => Log.Information("Called"));
        // platformIo.Platform_GetWindowMinimized = Marshal.GetFunctionPointerForDelegate(() => Log.Information("Called"));
        // platformIo.Platform_SetWindowTitle = Marshal.GetFunctionPointerForDelegate(() => Log.Information("Called"));

        var io = ImGui.GetIO();
        // io.BackendFlags |=
        //     // ImGuiBackendFlags.RendererHasVtxOffset
        //     ImGuiBackendFlags.HasMouseCursors
        //     | ImGuiBackendFlags.HasSetMousePos
        //     // | ImGuiBackendFlags.RendererHasViewports
        //     | ImGuiBackendFlags.PlatformHasViewports;
        //
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable | ImGuiConfigFlags.ViewportsEnable | ImGuiConfigFlags.NavEnableKeyboard;
        io.WantSaveIniSettings = true;
        ImGui.StyleColorsDark();

        Log.Information("ImGui Started");
    }

    protected virtual void OnUpdate(float deltaTime) {
        controller.Update(deltaTime);

        ImGui.DockSpaceOverViewport(ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode);
        Inspector.Render();
        Hierarchy.Render();

        if (ImGui.BeginMainMenuBar()) {
            // if (ImGui.BeginMenu("File")) {
            //     if (ImGui.MenuItem("New", "Ctrl+N")) {
            //         opened = true;
            //     }
            //     ImGui.EndMenu();
            // }
            
            if (ImGui.BeginMenu("View")) {
                if (ImGui.MenuItem("Inspector", "Ctrl+I")) {
                    Inspector.IsOpened = true;
                }
                
                if (ImGui.MenuItem("Debug Transform View Matrix")) {
                    Inspector.IsOpenedDebug = true;
                }
                ImGui.EndMenu();
            }
            
            ImGui.EndMainMenuBar();
        }
        
        //
        // ImGui.Begin("Foo Bar", ImGuiWindowFlags.DockNodeHost);
        // ImGui.Text("Hello world!");
        // // var str = new char[256];
        // // ImGui.InputText("Label", ref inputString, 32);
        // ImGui.End();
        //
        // ImGui.Begin("Tab window");
        // if (ImGui.BeginTabBar("Tab Bar")) {
        //     if (ImGui.BeginTabItem("Item 1")) {
        //         ImGui.Button("Buttin 1");
        //         ImGui.EndTabItem();
        //     }
        //     
        //     if (ImGui.BeginTabItem("Item 2")) {
        //         ImGui.Button("Buttin 2");
        //         ImGui.EndTabItem();
        //     }
        //     
        //     if (ImGui.BeginTabItem("Item 3")) {
        //         ImGui.Button("Buttin 3");
        //         ImGui.EndTabItem();
        //     }
        //     
        //     ImGui.EndTabBar();
        // }
        // ImGui.End();
        //

        ImGui.ShowDemoWindow();
        controller.Render();
    }

    // void _CreateWindow(ImGuiViewportPtr vp) {
    // Window.Create(WindowOptions.Default);
    // }
}


delegate void Platform_CreateWindow(ImGuiViewportPtr vp);

delegate void Platform_DestroyWindow(ImGuiViewportPtr vp);

delegate void Platform_ShowWindow(ImGuiViewportPtr vp);

delegate void Platform_SetWindowPos(ImGuiViewportPtr vp, Vector2 pos);

unsafe delegate void Platform_GetWindowPos(ImGuiViewportPtr vp, Vector2* outPos);

delegate void Platform_SetWindowSize(ImGuiViewportPtr vp, Vector2 size);

unsafe delegate void Platform_GetWindowSize(ImGuiViewportPtr vp, Vector2* outSize);

delegate void Platform_SetWindowFocus(ImGuiViewportPtr vp);

delegate byte Platform_GetWindowFocus(ImGuiViewportPtr vp);

delegate byte Platform_GetWindowMinimized(ImGuiViewportPtr vp);

delegate void Platform_SetWindowTitle(ImGuiViewportPtr vp, IntPtr title);
