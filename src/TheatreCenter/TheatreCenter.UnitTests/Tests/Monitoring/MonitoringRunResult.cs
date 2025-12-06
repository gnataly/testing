using System.Text.Json.Serialization;

namespace TheatreCenter.UnitTests.Tests.Monitoring;

public record MonitoringRunResult(
    bool TracingEnabled,
    bool ExtendedLogging,
    int Iterations,
    double DurationMs,
    double CpuMs,
    long MemoryDeltaBytes,
    long PeakWorkingSetBytes,
    DateTimeOffset CapturedAt)
{
    [JsonIgnore]
    public string Mode => $"Tracing:{TracingEnabled};ExtendedLogging:{ExtendedLogging}";
}
