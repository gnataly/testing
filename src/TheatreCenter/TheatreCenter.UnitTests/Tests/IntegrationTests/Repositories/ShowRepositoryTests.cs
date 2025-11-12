using Microsoft.EntityFrameworkCore;
using TheatreCenter.Data;
using TheatreCenter.Data.Repositories;
using TheatreCenter.Domain.Models;
using TheatreCenter.Tests.Fixtures;
using FluentAssertions;
using TheatreCenter.UnitTests.Tests.Database;
using TheatreCenter.UnitTests;
using Xunit;

namespace TheatreCenter.Tests.IntegrationTests.Repositories;

[Collection("Database collection")]
[Trait("Category", TestCategories.Integration)]
public class ShowRepositoryIt(DatabaseFixture db) : IClassFixture<DatabaseFixture>
{
    private readonly ShowFixture _showFixture = new ShowFixture();
    private readonly MusicalFixture _musicalFixture = new MusicalFixture();
    private ShowRepository CreateRepository()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;

        var context = new AppDbContext(options);
        return new ShowRepository(context);
    }

    [Fact]
    public async Task Show_FullCycle_WithUpcomingShows()
    {
        // Arrange
        var repo = CreateRepository();

        // Создаем мюзикл для показа
        var musical = _musicalFixture.CreateMusical(title: "Test Musical");

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;
        using var context = new AppDbContext(options);
        context.Musicals.Add(musical);
        await context.SaveChangesAsync();

        // Act 1 - создать показ
        var show = _showFixture.CreateShow(
            musicalId: musical.Id,
            date: DateTime.UtcNow.AddDays(7));

        await repo.AddAsync(show);

        // Assert 1 - проверить создание
        var created = await repo.GetByIdAsync(show.Id);
        created.Should().NotBeNull();
        created!.MusicalId.Should().Be(musical.Id);
        created.Date.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromSeconds(1));

        // Act 2 - обновить показ
        var newDate = DateTime.UtcNow.AddDays(14);
        created.Date = newDate;
        await repo.UpdateAsync(created);

        // Assert 2 - проверить обновление
        var updated = await repo.GetByIdAsync(show.Id);
        updated!.Date.Should().BeCloseTo(newDate, TimeSpan.FromSeconds(1));

        // Act 3 - получить показы по мюзиклу
        var musicalShows = await repo.GetByMusicalIdAsync(musical.Id);

        // Assert 3 - проверить фильтрацию по мюзиклу
        musicalShows.Should().ContainSingle(s => s.Id == show.Id);

        // Act 4 - получить предстоящие показы
        var upcomingShows = await repo.GetUpcomingShowsAsync();

        // Assert 4 - проверить получение предстоящих показов
        upcomingShows.Should().ContainSingle(s => s.Id == show.Id);

        // Act 5 - удалить показ
        await repo.RemoveAsync(updated);

        // Assert 5 - проверить удаление
        var deleted = await repo.GetByIdAsync(show.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task Show_GetUpcomingShows_ReturnsOnlyFutureShows()
    {
        // Arrange
        var repo = CreateRepository();

        var musical = _musicalFixture.CreateMusical();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;
        using var context = new AppDbContext(options);
        context.Musicals.Add(musical);
        await context.SaveChangesAsync();

        var futureShow = _showFixture.CreateShow(
            musicalId: musical.Id,
            date: DateTime.UtcNow.AddDays(1)); // Будущий показ

        var pastShow = _showFixture.CreateShow(
            musicalId: musical.Id,
            date: DateTime.UtcNow.AddDays(-3)); // Прошедший показ

        await repo.AddAsync(futureShow);
        await repo.AddAsync(pastShow);

        // Act
        var upcomingShows = await repo.GetUpcomingShowsAsync();

        // Assert
        upcomingShows.Should().ContainSingle(s => s.Id == futureShow.Id);
        upcomingShows.Should().NotContain(s => s.Id == pastShow.Id);

        // Cleanup
        await repo.RemoveAsync(futureShow);
        await repo.RemoveAsync(pastShow);
    }
}