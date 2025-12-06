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
using TheatreCenter.Domain.Interfaces.Repositories;

namespace TheatreCenter.UnitTests.Tests.IntegrationTests.Services;

[CollectionDefinition("Database collection")]
[Trait("Category", TestCategories.Integration)]
public class ShowServiceIt : IntegrationTestBase
{
    private readonly ShowFixture _showFixture = new ShowFixture();
    private readonly MusicalFixture _musicalFixture = new MusicalFixture();
    private readonly TheatreFixture _theatreFixture = new TheatreFixture();
    private MusicalRepository _musicalRepository;
    private TheatreRepository _theatreRepository;
    private ShowService _service;
    private Func<Task> _commitTransaction;
    private Func<Task> _rollbackTransaction;

    public ShowServiceIt(DatabaseFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    public override async Task InitializeAsync()
    {
        //await Fixture.WaitForDatabaseReadyAsync(TimeSpan.FromSeconds(30));
        var context = await Fixture.CreateTransactionalContextAsync();
        var showRepository = Fixture.CreateRepository<ShowRepository>(context);
        _musicalRepository = Fixture.CreateRepository<MusicalRepository>(context);
        _theatreRepository = Fixture.CreateRepository<TheatreRepository>(context);
        _service = new ShowService(showRepository, _musicalRepository, new NullLogger<ShowService>());

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
    public async Task Show_FullCycle_WithFixtures()
    {
        var theatre = _theatreFixture.CreateTheatre();
        await _theatreRepository.AddAsync(theatre);
        await _theatreRepository.SaveChangesAsync();

        var musical = _musicalFixture.CreateMusical(
            theatreId: theatre.Id);
        await _musicalRepository.AddAsync(musical);
        await _musicalRepository.SaveChangesAsync();

        var testShow = _showFixture.CreateShow(
            date: DateTime.UtcNow.AddDays(7),
            musicalId: musical.Id
        );

        var createdShow = await _service.CreateAsync(testShow);

        createdShow.Should().NotBeNull();
        createdShow.Id.Should().BeGreaterThan(0);
        createdShow.MusicalId.Should().Be(testShow.MusicalId);

        var retrievedShow = await _service.GetByIdAsync(createdShow.Id);
        retrievedShow.Should().NotBeNull();
        retrievedShow.MusicalId.Should().Be(testShow.MusicalId);

        var updatedShow = _showFixture.CreateShow(
            id: createdShow.Id,
            musicalId: testShow.MusicalId,
            date: DateTime.UtcNow.AddDays(14)
        );

        var updateResult = await _service.UpdateAsync(updatedShow);
        updateResult.Should().NotBeNull();
        updateResult.Date.Should().BeCloseTo(updatedShow.Date, TimeSpan.FromSeconds(1));

        var showsByMusical = await _service.GetByMusicalIdAsync(testShow.MusicalId);
        showsByMusical.Should().Contain(s => s.Id == createdShow.Id);

        var upcomingShows = await _service.GetUpcomingShowsAsync();
        upcomingShows.Should().Contain(s => s.Id == createdShow.Id);

        var allShows = await _service.GetAllAsync(new ShowFilter());
        allShows.Should().Contain(s => s.Id == createdShow.Id);

        var deleteResult = await _service.DeleteAsync(createdShow.Id);
        deleteResult.Should().BeTrue();
    }
}