using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using ILogger = Serilog.ILogger;

namespace TheatreCenter.Backend.WebAPI.Telemetry;

public static class OpenTelemetryServiceCollectionExtensions
{
    public static IServiceCollection AddTheatreCenterTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        ILogger logger)
    {
        var options = configuration.GetSection(TelemetryOptions.SectionName).Get<TelemetryOptions>() ?? new TelemetryOptions();
        services.Configure<TelemetryOptions>(configuration.GetSection(TelemetryOptions.SectionName));

        if (!options.TracingEnabled)
        {
            logger.Information("OpenTelemetry tracing is disabled by configuration.");
            return services;
        }

        var serviceName = string.IsNullOrWhiteSpace(options.ServiceName) ? "TheatreCenter.Backend" : options.ServiceName;

        services.AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                resource.AddService(
                    serviceName: serviceName,
                    serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown",
                    serviceInstanceId: Environment.MachineName);
            })
            .WithTracing(tracing =>
            {
                var probability = Math.Clamp(options.SamplingProbability, 0.0, 1.0);
                tracing
                    .SetSampler(new ParentBasedSampler(new TraceIdRatioBasedSampler(probability)))
                    .AddAspNetCoreInstrumentation(o =>
                    {
                        o.RecordException = true;
                    })
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation(o =>
                    {
                        o.SetDbStatementForText = true;
                        o.SetDbStatementForStoredProcedure = true;
                    })
                    .AddSource(Diagnostics.ActivitySourceName);

                tracing.AddJaegerExporter(exporter =>
                {
                    if (!string.IsNullOrWhiteSpace(options.JaegerEndpoint))
                    {
                        exporter.Endpoint = new Uri(options.JaegerEndpoint);
                    }
                });

                if (options.EnableConsoleExporter)
                {
                    tracing.AddConsoleExporter();
                }
            });

        logger.Information(
            "OpenTelemetry tracing is enabled; sampling probability: {SamplingProbability}; Jaeger endpoint: {JaegerEndpoint}",
            options.SamplingProbability,
            options.JaegerEndpoint ?? "default");

        return services;
    }
}
