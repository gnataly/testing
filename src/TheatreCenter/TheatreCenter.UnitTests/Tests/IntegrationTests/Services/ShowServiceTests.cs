using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheatreCenter.Data.Repositories;
using TheatreCenter.Data;
using TheatreCenter.Services.Services;
using TheatreCenter.Tests.Fixtures;
using TheatreCenter.UnitTests.Tests.Database;
using Xunit;
using FluentAssertions;

namespace TheatreCenter.UnitTests.Tests.IntegrationTests.Services;

[Collection("Database collection")]
[Trait("Category", TestCategories.Integration)]
public class ShowServiceIt(DatabaseFixture db) : IClassFixture<DatabaseFixture>
{
    private readonly ShowFixture _showFixture = new ShowFixture();
    private readonly MusicalFixture _musicalFixture = new MusicalFixture();

    private ShowService CreateService()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;

        var context = new AppDbContext(options);
        var showRepository = new ShowRepository(context);
        var musicalRepository = new MusicalRepository(context);
        return new ShowService(showRepository, musicalRepository);
    }

    [Fact]
    public async Task Show_FullCycle_WithFixtures()
    {
        var service = CreateService();

        // Создаем театр и мюзикл для показа
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;

        var musical = _musicalFixture.CreateMusical(
            title: "Test Musical"
        );

        await using (var context = new AppDbContext(options))
        {
            await context.Musicals.AddAsync(musical);
            await context.SaveChangesAsync();
        }

        // Используем фикстуру для генерации показа
        var testShow = _showFixture.CreateShow(
            musicalId: musical.Id,
            date: DateTime.UtcNow.AddDays(7)
        );

        // Act 1 — создание показа
        var createdShow = await service.CreateAsync(testShow);

        // Assert 1 — проверка создания
        createdShow.Should().NotBeNull();
        createdShow.Id.Should().BeGreaterThan(0);
        createdShow.MusicalId.Should().Be(musical.Id);

        // Act 2 — получение показа по ID
        var retrievedShow = await service.GetByIdAsync(createdShow.Id);
        retrievedShow.Should().NotBeNull();
        retrievedShow.MusicalId.Should().Be(musical.Id);

        // Act 3 — обновление показа
        var updatedShow = _showFixture.CreateShow(
            id: createdShow.Id,
            musicalId: musical.Id,
            date: DateTime.UtcNow.AddDays(14)
        );

        var updateResult = await service.UpdateAsync(updatedShow);
        updateResult.Should().NotBeNull();
        updateResult.Date.Should().BeCloseTo(updatedShow.Date, TimeSpan.FromSeconds(1));

        // Act 4 — получение показов по мюзиклу
        var showsByMusical = await service.GetByMusicalIdAsync(musical.Id);
        showsByMusical.Should().Contain(s => s.Id == createdShow.Id);

        // Act 5 — получение предстоящих показов
        var upcomingShows = await service.GetUpcomingShowsAsync();
        upcomingShows.Should().Contain(s => s.Id == createdShow.Id);

        // Act 6 — получение всех показов
        var allShows = await service.GetAllAsync();
        allShows.Should().Contain(s => s.Id == createdShow.Id);

        // Act 7 — удаление показа
        var deleteResult = await service.DeleteAsync(createdShow.Id);
        deleteResult.Should().BeTrue();

        //// Assert 7 — проверка удаления
        //var deletedShow = await service.GetByIdAsync(createdShow.Id);
        //deletedShow.Should().BeNull();
    }
}
