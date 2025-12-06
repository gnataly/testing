using FluentAssertions;
using Xunit;

namespace TheatreCenter.UnitTests.Tests.Monitoring;

public class MonitoringOverheadTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    [Trait("Category", "Unit")]
    public async Task Collects_resource_usage_snapshots(bool tracingEnabled, bool extendedLogging)
    {
        await using var scenario = new MonitoringScenario(tracingEnabled, extendedLogging);
        var result = await scenario.RunAsync(iterations: 1500);

        result.Iterations.Should().Be(1500);
        MonitoringReportWriter.AppendResult(result);
    }
}
