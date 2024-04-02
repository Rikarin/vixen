using ImGuiNET;
using System.Drawing;
using System.Numerics;
using Vixen.Diagnostics;

namespace Vixen.Editor.Panes;

sealed class ProfilerPane : Pane {
    static readonly Color Cyan = Color.FromArgb(255, 0, 187, 161);
    static readonly Color LightBlue = Color.FromArgb(255, 1, 151, 235);
    static readonly Color Pink = Color.FromArgb(255, 255, 25, 163);
    static readonly Color Yellow = Color.FromArgb(255, 255, 194, 5);

    public ProfilerPane() : base("Profiler") { }

    void PlotLineMetric(string name, Color? color = null, float? max = null) {
        if (Profiler.TryGetMetrics(name, out var entry)) {
            ImGui.Text(name);
            // ImGui.Text($"{entry.DisplayName} [Min: {entry.Min:f3} Max: {entry.Max:f3}] ({entry.DisplayUnits})");

            if (color.HasValue) {
                ImGui.PushStyleColor(ImGuiCol.PlotLines, color.Value.ToVector4());
            }

            var data = entry.Select(x => (float)x).ToArray();

            if (data.Length == 0) {
                return;
            }

            ImGui.PlotLines(
                $"##{name}",
                ref data[0],
                data.Length,
                0,
                string.Empty,
                0,
                max ?? float.MaxValue
            );

            if (color.HasValue) {
                ImGui.PopStyleColor();
            }
        }
    }

    void PlotLine(string name, Color? color = null, float? max = null) {
        if (ProfilerData.Data.TryGetValue(name, out var entry)) {
            ImGui.Text($"{entry.DisplayName} [Min: {entry.Min:f3} Max: {entry.Max:f3}] ({entry.DisplayUnits})");

            if (color.HasValue) {
                ImGui.PushStyleColor(ImGuiCol.PlotLines, color.Value.ToVector4());
            }

            ImGui.PlotLines(
                $"##{name}",
                ref entry.MeanData[0],
                entry.MeanData.Length,
                0,
                string.Empty,
                0,
                max ?? float.MaxValue
            );

            if (color.HasValue) {
                ImGui.PopStyleColor();
            }
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

        // Log.Information("Debug: {Variable}", Profiler.Metrics.Keys);

        ImGui.Text("CPU");
        ImGui.Separator();
        PlotLine("cpu-usage", Cyan);
        PlotLine("monitor-lock-contention-count");
        PlotProgress("cpu-usage", 100);

        ImGui.Spacing();
        ImGui.Spacing();
        ImGui.Text("Engine");
        ImGui.Separator();
        PlotLineMetric("Rin.Renderer.SubmitCount", Cyan);
        PlotLineMetric("Rin.Renderer.SubmitDisposalCount", Cyan);
        PlotLineMetric("Rin.Application.WorkTime", LightBlue);
        PlotLineMetric("Rin.Application.WaitTime", Yellow);
        PlotLineMetric("Rin.Renderer.WorkTime", Cyan);
        PlotLineMetric("Rin.Renderer.WaitTime", Pink);

        ImGui.Spacing();
        ImGui.Spacing();
        ImGui.Text("Garbage Collector");
        ImGui.Separator();
        PlotLine("time-in-gc", Color.Green);
        PlotLine("gc-heap-size", Color.DarkOrange);
        PlotLine("gc-fragmentation", Color.Maroon);
        PlotLine("gc-committed", Color.Green);
        PlotLine("gen-0-size", Color.Green);
        PlotLine("gen-1-size", Color.Green);
        PlotLine("gen-2-size", Color.Green);
        PlotLine("gen-0-gc-count", Color.Green);
        PlotLine("gen-1-gc-count", Color.Green);
        PlotLine("gen-2-gc-count", Color.Green);

        ImGui.Spacing();
        ImGui.Spacing();
        ImGui.Text("Thread Pool");
        ImGui.Separator();
        PlotLine("threadpool-thread-count", Color.Olive);
        PlotLine("threadpool-queue-length", Color.LightCoral);
        PlotLine("threadpool-completed-items-count", Color.Aqua);

        ImGui.Spacing();
        ImGui.Spacing();
        ImGui.Text("Just in Time");
        ImGui.Separator();
        PlotLine("time-in-jit", Color.MediumPurple);
        PlotLine("methods-jitted-count", Color.MediumPurple);
        PlotLine("il-bytes-jitted", Color.MediumPurple);
        
        ImGui.Spacing();
        ImGui.Spacing();
        ImGui.Text("Misc");
        ImGui.Separator();
        PlotLine("working-set", Color.MediumPurple);
        PlotLine("monitor-lock-contention-count", Color.MediumPurple);
        PlotLine("alloc-rate", Color.MediumPurple);
        PlotLine("active-timer-count", Color.MediumPurple);
        PlotLine("exception-count", Color.MediumPurple);
        PlotLine("loh-size", Color.MediumPurple);
        PlotLine("poh-size", Color.MediumPurple);
        PlotLine("assembly-count", Color.MediumPurple);

        ImGui.PopItemWidth();
    }
}
