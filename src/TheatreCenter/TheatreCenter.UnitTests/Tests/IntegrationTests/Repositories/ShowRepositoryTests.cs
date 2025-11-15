using Microsoft.EntityFrameworkCore;
using TheatreCenter.Data;
using TheatreCenter.Data.Repositories;
using TheatreCenter.Domain.Models;
using TheatreCenter.Tests.Fixtures;
using FluentAssertions;
using TheatreCenter.UnitTests.Tests.Database;
using TheatreCenter.UnitTests;
using Xunit;
using Xunit.Abstractions;
using AutoFixture;
using TheatreCenter.UnitTests.Tests.IntegrationTests;

namespace TheatreCenter.Tests.IntegrationTests.Repositories;

[Collection("Database collection")]
[Trait("Category", TestCategories.Integration)]
public class ShowRepositoryIt : IntegrationTestBase
{
    private readonly ShowFixture _showFixture = new ShowFixture();
    private readonly MusicalFixture _musicalFixture = new MusicalFixture();
    private readonly TheatreFixture _theatreFixture = new TheatreFixture();
    private ShowRepository _showRepository;
    private MusicalRepository _musicalRepository;
    private TheatreRepository _theatreRepository;
    private Func<Task> _commitTransaction;
    private Func<Task> _rollbackTransaction;

    public ShowRepositoryIt(DatabaseFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    public override async Task InitializeAsync()
    {
        var context = await Fixture.CreateTransactionalContextAsync();

        _showRepository = Fixture.CreateRepository<ShowRepository>(context);
        _musicalRepository = Fixture.CreateRepository<MusicalRepository>(context);
        _theatreRepository = Fixture.CreateRepository<TheatreRepository>(context);

        _commitTransaction = async () => { await context.Database.CommitTransactionAsync(); await context.DisposeAsync(); };
        _rollbackTransaction = async () => { await context.Database.RollbackTransactionAsync(); await context.DisposeAsync(); };
    }

    public override async Task DisposeAsync()
    {
        await _rollbackTransaction();
    }

    [Fact]
    public async Task Show_FullCycle_WithUpcomingShows()
    {
        var theatre = _theatreFixture.CreateTheatre();
        await _theatreRepository.AddAsync(theatre);

        var musical = _musicalFixture.CreateMusical(theatreId: theatre.Id);
        await _musicalRepository.AddAsync(musical);

        var show = _showFixture.CreateShow(
            musicalId: musical.Id,
            date: DateTime.UtcNow.AddDays(7));

        await _showRepository.AddAsync(show);

        var created = await _showRepository.GetByIdAsync(show.Id);
        created.Should().NotBeNull();
        created!.MusicalId.Should().Be(show.MusicalId);
        created.Date.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromSeconds(1));

        var newDate = DateTime.UtcNow.AddDays(14);
        created.Date = newDate;
        await _showRepository.UpdateAsync(created);

        var updated = await _showRepository.GetByIdAsync(show.Id);
        updated!.Date.Should().BeCloseTo(newDate, TimeSpan.FromSeconds(1));

        var musicalShows = await _showRepository.GetByMusicalIdAsync(show.MusicalId);

        musicalShows.Should().ContainSingle(s => s.Id == show.Id);

        var upcomingShows = await _showRepository.GetUpcomingShowsAsync();

        upcomingShows.Should().ContainSingle(s => s.Id == show.Id);

        await _showRepository.RemoveAsync(updated);

        var deleted = await _showRepository.GetByIdAsync(show.Id);
        deleted.Should().BeNull();
    }
}