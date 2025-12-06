using Xunit.Abstractions;
using TheatreCenter.UnitTests.Tests.Database;
using Xunit;

namespace TheatreCenter.UnitTests.Tests.IntegrationTests;

[CollectionDefinition("Database collection")]
[Trait("Category", TestCategories.Integration)]
public abstract class IntegrationTestBase : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    protected readonly DatabaseFixture Fixture;
    protected readonly ITestOutputHelper Output;

    public IntegrationTestBase(DatabaseFixture fixture, ITestOutputHelper output)
    {
        Fixture = fixture;
        Output = output;
        Fixture.Output = output;
    }

    public virtual Task InitializeAsync() => Task.CompletedTask;

    public virtual Task DisposeAsync() => Task.CompletedTask;
}
