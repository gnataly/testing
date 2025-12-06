using System.Diagnostics;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace TheatreCenter.UnitTests.Tests.Monitoring;

public sealed class MonitoringScenario : IAsyncDisposable
{
    private readonly bool _tracingEnabled;
    private readonly bool _extendedLogging;
    private readonly ActivitySource _activitySource = new("TheatreCenter.MonitoringScenario");
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger _logger;
    private readonly List<Activity> _capturedActivities = new();
    private readonly TracerProvider? _tracerProvider;

    public MonitoringScenario(bool tracingEnabled, bool extendedLogging)
    {
        _tracingEnabled = tracingEnabled;
        _extendedLogging = extendedLogging;

        _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(_extendedLogging ? LogLevel.Debug : LogLevel.Information);
            builder.AddConsole();
        });
        _logger = _loggerFactory.CreateLogger<MonitoringScenario>();

        if (_tracingEnabled)
        {
            _tracerProvider = Sdk.CreateTracerProviderBuilder()
                .ConfigureResource(resource => resource.AddService("TheatreCenter.UnitTests", serviceInstanceId: Environment.MachineName))
                .AddSource(_activitySource.Name)
                .AddInMemoryExporter(_capturedActivities)
                .Build();
        }
    }

    public async Task<MonitoringRunResult> RunAsync(int iterations, CancellationToken cancellationToken = default)
    {
        var process = Process.GetCurrentProcess();
        process.Refresh();

        var cpuBefore = process.TotalProcessorTime;
        var memoryBefore = GC.GetTotalMemory(forceFullCollection: true);

        var stopwatch = Stopwatch.StartNew();
        for (var i = 0; i < iterations; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var activity = _tracingEnabled
                ? _activitySource.StartActivity("monitoring.iteration", ActivityKind.Internal)
                : null;

            var payload = Math.Abs(Math.Sin(i * 0.01)) * (i + 1);
            var calculated = Math.Sqrt(payload);

            if (_extendedLogging)
            {
                _logger.LogDebug("Iteration {Iteration} payload {Payload} result {Result}", i, payload, calculated);
            }
            else if (i % 500 == 0)
            {
                _logger.LogInformation("Iteration checkpoint {Iteration}", i);
            }

            activity?.SetTag("payload", payload);
            activity?.SetTag("result", calculated);

            await Task.Yield();
        }

        stopwatch.Stop();
        process.Refresh();
        var cpuAfter = process.TotalProcessorTime;
        var memoryAfter = GC.GetTotalMemory(forceFullCollection: true);

        return new MonitoringRunResult(
            _tracingEnabled,
            _extendedLogging,
            iterations,
            stopwatch.Elapsed.TotalMilliseconds,
            (cpuAfter - cpuBefore).TotalMilliseconds,
            memoryAfter - memoryBefore,
            process.PeakWorkingSet64,
            DateTimeOffset.UtcNow);
    }

    public ValueTask DisposeAsync()
    {
        _tracerProvider?.Dispose();
        _loggerFactory.Dispose();
        return ValueTask.CompletedTask;
    }
}
