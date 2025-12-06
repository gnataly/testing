using System.Text.Json;
using System.Text;

namespace TheatreCenter.UnitTests.Tests.Monitoring;

public static class MonitoringReportWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static string GetReportPath()
    {
        var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        return Path.Combine(repoRoot, "artifacts", "monitoring-report.json");
    }

    public static string GetMarkdownPath()
    {
        var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        return Path.Combine(repoRoot, "artifacts", "monitoring-report.md");
    }

    public static void AppendResult(MonitoringRunResult result)
    {
        var reportPath = GetReportPath();
        Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);

        List<MonitoringRunResult> reportItems;
        if (File.Exists(reportPath))
        {
            var existingContent = File.ReadAllText(reportPath);
            reportItems = JsonSerializer.Deserialize<List<MonitoringRunResult>>(existingContent, JsonOptions) ?? new List<MonitoringRunResult>();
        }
        else
        {
            reportItems = new List<MonitoringRunResult>();
        }

        reportItems.Add(result);
        var json = JsonSerializer.Serialize(reportItems, JsonOptions);
        File.WriteAllText(reportPath, json);

        WriteMarkdown(reportItems);
    }

    private static void WriteMarkdown(IEnumerable<MonitoringRunResult> reportItems)
    {
        var sorted = reportItems
            .OrderBy(r => r.CapturedAt)
            .ToList();

        var sb = new StringBuilder();
        sb.AppendLine("# Monitoring Overhead Report");
        sb.AppendLine();
        sb.AppendLine($"Generated: {DateTimeOffset.UtcNow:O}");
        sb.AppendLine();
        sb.AppendLine("| Mode | Iterations | Duration (ms) | CPU (ms) | Memory Δ (KB) | Peak WS (MB) | CapturedAt |");
        sb.AppendLine("| --- | ---: | ---: | ---: | ---: | ---: | --- |");

        foreach (var item in sorted)
        {
            var memoryKb = item.MemoryDeltaBytes / 1024.0;
            var peakMb = item.PeakWorkingSetBytes / 1024.0 / 1024.0;
            sb.AppendLine(
                $"| Tracing {(item.TracingEnabled ? "On" : "Off")}, Extended {(item.ExtendedLogging ? "On" : "Off")} " +
                $"| {item.Iterations} " +
                $"| {item.DurationMs:F2} " +
                $"| {item.CpuMs:F0} " +
                $"| {memoryKb:F1} " +
                $"| {peakMb:F1} " +
                $"| {item.CapturedAt:O} |");
        }

        File.WriteAllText(GetMarkdownPath(), sb.ToString());
    }
}
