using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TheatreCenter.Data.Repositories;
using TheatreCenter.Data;
using TheatreCenter.Services.Services;
using TheatreCenter.Tests.Fixtures;
using TheatreCenter.UnitTests.Tests.Database;
using Xunit;
using FluentAssertions;
using Xunit.Abstractions;
using TheatreCenter.UnitTests;

namespace TheatreCenter.UnitTests.Tests.IntegrationTests.Services;

[Collection("Database collection")]
[Trait("Category", TestCategories.Integration)]
public class ShowServiceIt : IntegrationTestBase
{
    private readonly ShowFixture _showFixture = new ShowFixture();
    private readonly MusicalFixture _musicalFixture = new MusicalFixture();
    private ShowService _service;
    private Func<Task> _commitTransaction;
    private Func<Task> _rollbackTransaction;

    public ShowServiceIt(DatabaseFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    public override async Task InitializeAsync()
    {
        var context = await Fixture.CreateTransactionalContextAsync();
        var showRepository = Fixture.CreateRepository<ShowRepository>(context);
        var musicalRepository = Fixture.CreateRepository<MusicalRepository>(context);
        _service = new ShowService(showRepository, musicalRepository);

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
        //var context = await Fixture.CreateTransactionalContextAsync();

        //var musical = _musicalFixture.CreateMusical();
        //await context.Musicals.AddAsync(musical);
        //await context.SaveChangesAsync();
        //await context.Database.CommitTransactionAsync();

        var testShow = _showFixture.CreateShow(
            date: DateTime.UtcNow.AddDays(7)
        );

        // Act 1 — создание показа
        var createdShow = await _service.CreateAsync(testShow);

        // Assert 1 — проверка создания
        createdShow.Should().NotBeNull();
        createdShow.Id.Should().BeGreaterThan(0);
        createdShow.MusicalId.Should().Be(testShow.MusicalId);

        // Act 2 — получение показа по ID
        var retrievedShow = await _service.GetByIdAsync(createdShow.Id);
        retrievedShow.Should().NotBeNull();
        retrievedShow.MusicalId.Should().Be(testShow.MusicalId);

        // Act 3 — обновление показа
        var updatedShow = _showFixture.CreateShow(
            id: createdShow.Id,
            musicalId: testShow.MusicalId,
            date: DateTime.UtcNow.AddDays(14)
        );

        var updateResult = await _service.UpdateAsync(updatedShow);
        updateResult.Should().NotBeNull();
        updateResult.Date.Should().BeCloseTo(updatedShow.Date, TimeSpan.FromSeconds(1));

        // Act 4 — получение показов по мюзиклу
        var showsByMusical = await _service.GetByMusicalIdAsync(testShow.MusicalId);
        showsByMusical.Should().Contain(s => s.Id == createdShow.Id);

        // Act 5 — получение предстоящих показов
        var upcomingShows = await _service.GetUpcomingShowsAsync();
        upcomingShows.Should().Contain(s => s.Id == createdShow.Id);

        // Act 6 — получение всех показов
        var allShows = await _service.GetAllAsync();
        allShows.Should().Contain(s => s.Id == createdShow.Id);

        // Act 7 — удаление показа
        var deleteResult = await _service.DeleteAsync(createdShow.Id);
        deleteResult.Should().BeTrue();
    }
}