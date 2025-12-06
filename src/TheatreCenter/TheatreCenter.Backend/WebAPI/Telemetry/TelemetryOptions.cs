namespace TheatreCenter.Backend.WebAPI.Telemetry;

public class TelemetryOptions
{
    public const string SectionName = "Telemetry";

    public bool TracingEnabled { get; set; } = true;

    public double SamplingProbability { get; set; } = 1.0;

    public string? ServiceName { get; set; }

    public string? JaegerEndpoint { get; set; }

    public bool EnableConsoleExporter { get; set; }
}
