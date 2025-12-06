using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TheatreCenter.Data.Repositories;
using TheatreCenter.Data;
using TheatreCenter.Domain.Models;
using TheatreCenter.Services.Services;
using TheatreCenter.Tests.Fixtures;
using TheatreCenter.UnitTests.Tests.Database;
using Xunit;
using FluentAssertions;
using Xunit.Abstractions;
using TheatreCenter.UnitTests;

namespace TheatreCenter.UnitTests.Tests.IntegrationTests.Services;

[CollectionDefinition("Database collection")]
[Trait("Category", TestCategories.Integration)]
public class TheatreServiceIt : IntegrationTestBase
{
    private readonly TheatreFixture _theatreFixture = new TheatreFixture();
    private TheatreService _service;
    private Func<Task> _commitTransaction;
    private Func<Task> _rollbackTransaction;

    public TheatreServiceIt(DatabaseFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    public override async Task InitializeAsync()
    {
        //await Fixture.WaitForDatabaseReadyAsync(TimeSpan.FromSeconds(30));
        var context = await Fixture.CreateTransactionalContextAsync();
        var theatreRepository = Fixture.CreateRepository<TheatreRepository>(context);
        var accountRepository = Fixture.CreateRepository<AccountRepository>(context);
        _service = new TheatreService(theatreRepository, accountRepository, new NullLogger<TheatreService>());

        _commitTransaction = async () => {
            await context.Database.CommitTransactionAsync();
            await context.DisposeAsync();
        };
        _rollbackTransaction = async () => {
            await context.Database.RollbackTransactionAsync();
            await context.DisposeAsync();
        };
    }

    public override async Task DisposeAsync()
    {
        await _rollbackTransaction();
    }

    [Fact]
    public async Task Theatre_FullCycle_WithFixtures()
    {

        var testTheatre = _theatreFixture.CreateTheatre();

        var createdTheatre = await _service.CreateTheatreAsync(testTheatre);

        createdTheatre.Should().NotBeNull();
        createdTheatre.Id.Should().BeGreaterThan(0);
        createdTheatre.Name.Should().Be(testTheatre.Name);

        var retrievedTheatre = await _service.GetTheatreByIdAsync(createdTheatre.Id);
        retrievedTheatre.Should().NotBeNull();
        retrievedTheatre.Name.Should().Be(testTheatre.Name);


        createdTheatre.Name = createdTheatre.Name + "123";

        var updateResult = await _service.UpdateTheatreAsync(createdTheatre);
        updateResult.Should().NotBeNull();
        updateResult.Name.Should().Be(createdTheatre.Name);

        var allTheatres = await _service.GetAllTheatresAsync(new TheatreFilter(), null);
        allTheatres.Should().Contain(t => t.Id == createdTheatre.Id);

        var deleteResult = await _service.DeleteTheatreAsync(createdTheatre.Id);
        deleteResult.Should().BeTrue();
    }
}