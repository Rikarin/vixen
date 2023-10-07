using ImGuiNET;
using System.Numerics;

namespace Rin.Editor.Panes;

sealed class ProfilerPane : Pane {
    public ProfilerPane() : base("Profiler") { }

    void PlotLine(string name, float? max = null) {
        if (ProfilerData.Data.TryGetValue(name, out var entry)) {
            ImGui.Text($"{entry.DisplayName} [Min: {entry.Min:f3} Max: {entry.Max:f3}] ({entry.DisplayUnits})");
            ImGui.PlotLines(
                $"##{name}",
                ref entry.MeanData[0],
                entry.MeanData.Length,
                0,
                string.Empty,
                0,
                max ?? float.MaxValue
            );
        }
    }
    
    void PlotProgress(string name, float max) {
        if (ProfilerData.Data.TryGetValue(name, out var entry)) {
            ImGui.Text(entry.DisplayName);
            ImGui.ProgressBar(entry.MeanData[^1] / max, Vector2.Zero);
        }
    }

    protected override void OnRender() {
        ImGui.PushItemWidth(-1);
        PlotLine("cpu-usage");
        PlotProgress("cpu-usage", 100);
        ImGui.Spacing();
        ImGui.Separator();
        
        PlotLine("main-thread-work-time");
        PlotLine("main-thread-wait-time");
        PlotLine("render-work-time");
        PlotLine("render-wait-time");

        ImGui.Spacing();
        ImGui.Separator();
        PlotLine("time-in-gc");
        PlotLine("gc-heap-size");
        PlotLine("gc-fragmentation");
        PlotLine("gc-committed");
        PlotLine("gen-0-size");
        PlotLine("gen-1-size");
        PlotLine("gen-2-size");
        PlotLine("gen-0-gc-count");
        PlotLine("gen-1-gc-count");
        PlotLine("gen-2-gc-count");
        
        ImGui.Spacing();
        ImGui.Separator();
        PlotLine("threadpool-thread-count");
        PlotLine("threadpool-queue-length");
        PlotLine("threadpool-completed-items-count");
        
        ImGui.Spacing();
        ImGui.Separator();
        PlotLine("time-in-jit");
        PlotLine("methods-jitted-count");
        PlotLine("il-bytes-jitted");
        
        // working-set: 253.181952
        // monitor-lock-contention-count: 355
        // alloc-rate: 10828320
        // active-timer-count: 0
        // exception-count: 0
        // loh-size: 319544
        // poh-size: 97264
        // assembly-count: 83

        ImGui.PopItemWidth();
        

        // ImPlot.BeginPlot("foo bar");
        // ImPlot.EndPlot();
        // ImGui.ProgressBar(0.5f, Vector2.Zero);
    }
}
