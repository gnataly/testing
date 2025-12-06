using System.Diagnostics;

namespace TheatreCenter.Backend.WebAPI.Telemetry;

public static class Diagnostics
{
    public const string ActivitySourceName = "TheatreCenter.Backend";

    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);
}
