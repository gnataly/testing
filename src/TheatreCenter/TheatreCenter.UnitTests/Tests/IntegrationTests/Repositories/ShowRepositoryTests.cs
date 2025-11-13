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
    private ShowRepository _repository;
    private Func<Task> _commitTransaction;
    private Func<Task> _rollbackTransaction;

    public ShowRepositoryIt(DatabaseFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    public override async Task InitializeAsync()
    {
        var (repository, commit, rollback) = await Fixture.CreateTransactionalRepositoryAsync<ShowRepository>();
        _repository = repository;
        _commitTransaction = commit;
        _rollbackTransaction = rollback;
    }

    public override async Task DisposeAsync()
    {
        await _rollbackTransaction();
    }

    [Fact]
    public async Task Show_FullCycle_WithUpcomingShows()
    {
        //// Arrange - Создаем все необходимые сущности с нуля
        //var context = await Fixture.CreateTransactionalContextAsync();

        //// Создаем театр
        //var theatre = _theatreFixture.CreateTheatre();
        //context.Theatres.Add(theatre);
        //await context.SaveChangesAsync();

        //// Создаем мюзикл для показа
        //var musical = _musicalFixture.CreateMusical(
        //    theatreId: theatre.Id);
        //context.Musicals.Add(musical);
        //await context.SaveChangesAsync();
        //await context.Database.CommitTransactionAsync();

        // Act 1 - создать показ
        var show = _showFixture.CreateShow(date: DateTime.UtcNow.AddDays(7));

        await _repository.AddAsync(show);

        // Assert 1 - проверить создание
        var created = await _repository.GetByIdAsync(show.Id);
        created.Should().NotBeNull();
        created!.MusicalId.Should().Be(show.MusicalId);
        created.Date.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromSeconds(1));

        // Act 2 - обновить показ
        var newDate = DateTime.UtcNow.AddDays(14);
        created.Date = newDate;
        await _repository.UpdateAsync(created);

        // Assert 2 - проверить обновление
        var updated = await _repository.GetByIdAsync(show.Id);
        updated!.Date.Should().BeCloseTo(newDate, TimeSpan.FromSeconds(1));

        // Act 3 - получить показы по мюзиклу
        var musicalShows = await _repository.GetByMusicalIdAsync(show.MusicalId);

        // Assert 3 - проверить фильтрацию по мюзиклу
        musicalShows.Should().ContainSingle(s => s.Id == show.Id);

        // Act 4 - получить предстоящие показы
        var upcomingShows = await _repository.GetUpcomingShowsAsync();

        // Assert 4 - проверить получение предстоящих показов
        upcomingShows.Should().ContainSingle(s => s.Id == show.Id);

        // Act 5 - удалить показ
        await _repository.RemoveAsync(updated);

        // Assert 5 - проверить удаление
        var deleted = await _repository.GetByIdAsync(show.Id);
        deleted.Should().BeNull();
    }
}