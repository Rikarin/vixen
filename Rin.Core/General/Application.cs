using Rin.Core.Abstractions;
using Rin.Core.Diagnostics;
using Rin.Platform.Silk;
using Rin.Platform.Vulkan;
using Rin.Rendering;
using Serilog;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using Key = Rin.InputSystem.Key;
using Profiler = Rin.Diagnostics.Profiler;

[assembly: InternalsVisibleTo("Rin.Editor")]

namespace Rin.Core.General;

public class Application : IApplication, IDisposable {
    internal static Application Current = null!;

    readonly ILogger log = Log.ForContext<Application>();
    readonly IRenderThread renderThread;
    readonly Stopwatch timer = new();
    readonly ConcurrentQueue<Action> mainThreadQueue = new();

    public bool IsRunning { get; private set; }
    public bool IsMinimized { get; private set; }
    public Window MainWindow { get; }

    public Application(ApplicationOptions options) {
        Current = this;
        InputContainer.inputManager = new();
        InputContainer.inputManager.Initialize(this);

        using var initializationProfileScope = ApplicationProfiling.StartInitialization();
        Renderer.Options.FramesInFlight = 3; // TODO: this needs to be loaded based on number of images in swapchain

        renderThread = new RenderThread(options.ThreadingPolicy);
        renderThread.Run();

        // TODO: stuff

        MainWindow = new(
            o => {
                o.Title = options.Name;
                o.Size = options.WindowSize;
                o.VSync = options.VSync;
            }
        );

        MainWindow.Resize += OnWindowResize;
        MainWindow.Closing += OnWindowClose;

        var vulkanRenderer = new VulkanRenderer();
        Renderer.Initialize(MainWindow.Handle.Swapchain, vulkanRenderer);
        renderThread.Pump();

        // Setup Renderer.SetConfig (static)
    }

    public void Run() {
        IsRunning = true;
        var silkWindow = MainWindow.Handle as SilkWindow;
        timer.Start();

        while (IsRunning) {
            Time.DeltaTime = timer.ElapsedMilliseconds / 1000f;
            timer.Restart();

            using (var _ = ApplicationProfiling.StartWaitTime()) {
                renderThread.BlockUntilRenderComplete();
            }

            log.Verbose("============= APPLICATION ======================");
            silkWindow.silkWindow.DoEvents();
            InputContainer.inputManager.Update();

            renderThread.NextFrame();
            renderThread.Kick();

            // TODO: if not minimized
            if (!IsMinimized) {
                using var workProfilingScope = ApplicationProfiling.StartWorkTime();

                Renderer.Submit(MainWindow.Handle.Swapchain.BeginFrame);
                Renderer.BeginFrame();

                var systems = SceneManager.ActiveScene!.Systems;
                systems.BeforeUpdate(Time.DeltaTime);
                systems.Update(Time.DeltaTime);
                systems.AfterUpdate(Time.DeltaTime);
                
                ExecuteMainThreadQueue();
                Update?.Invoke();

                Renderer.EndFrame();
                Renderer.Submit(MainWindow.Handle.Swapchain.Present);

                Renderer.IncreaseCurrentFrameIndex();
                
                // TODO: move this to Input class
                // silkWindow.ResetInput();

                // silkWindow.silkWindow.DoUpdate();
                // silkWindow.silkWindow.DoRender();

                if (InputContainer.inputManager.IsKeyPressed(Key.Escape)) {
                    IsRunning = false;
                }
            }
        }

        renderThread.Terminate();
    }

    public static Application CreateDefault(Action<ApplicationOptions>? configureOptions = null) {
        var options = new ApplicationOptions {
            Name = "Rin Engine", ThreadingPolicy = ThreadingPolicy.MultiThreaded, VSync = true
        };
        configureOptions?.Invoke(options);

        return new(options);
    }

    public void Dispose() {
        renderThread.Dispose();
        
        Renderer.Shutdown();
        // Layer detach

        MainWindow.Resize -= OnWindowResize;
        MainWindow.Closing -= OnWindowClose;
        
        Profiler.Shutdown();
    }

    public static void InvokeOnMainThread(Action action) {
        Current.mainThreadQueue.Enqueue(action);
    }

    void OnWindowResize(Size newSize) {
        Log.Information("OnResize: {Variable}", newSize);
        Renderer.Submit(() => MainWindow.Handle.Swapchain.OnResize(newSize));
    }

    void OnWindowClose() {
        Log.Information("on close");
        IsRunning = false;
    }

    void ExecuteMainThreadQueue() {
        while (mainThreadQueue.TryDequeue(out var action)) {
            try {
                
                action();
            } catch (Exception e) {
                log.Error(e, "Main Thread Queue");
            }
        }
    }

    public event Action? Update;
}