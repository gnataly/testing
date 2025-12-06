namespace TheatreCenter.Backend.WebAPI.Telemetry;

public class LoggingOptions
{
    public const string SectionName = "LoggingOptions";

    public bool Extended { get; set; }

    public string? ExtendedFilePath { get; set; } = "logs/theatrecenter-extended-.txt";
}
