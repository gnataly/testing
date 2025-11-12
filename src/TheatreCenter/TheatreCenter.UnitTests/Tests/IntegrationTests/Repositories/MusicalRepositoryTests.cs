using Microsoft.EntityFrameworkCore;
using TheatreCenter.Data;
using TheatreCenter.Data.Repositories;
using TheatreCenter.Domain.Enums;
using TheatreCenter.Domain.Models;
using TheatreCenter.Tests.Fixtures;
using FluentAssertions;
using TheatreCenter.UnitTests.Tests.Database;
using TheatreCenter.UnitTests;
using Xunit;

namespace TheatreCenter.Tests.IntegrationTests.Repositories;

[Collection("Database collection")]
[Trait("Category", TestCategories.Integration)]
public class MusicalRepositoryIt(DatabaseFixture db) : IClassFixture<DatabaseFixture>
{
    private readonly MusicalFixture _musicalFixture = new MusicalFixture();
    private readonly TheatreFixture _theatreFixture = new TheatreFixture();

    private MusicalRepository CreateRepository()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;

        var context = new AppDbContext(options);
        return new MusicalRepository(context);
    }

    [Fact]
    public async Task Musical_FullCycle_WithAgeRestriction()
    {
        // Arrange
        var repo = CreateRepository();

        // Создаем театр для мюзикла
        var theatre = _theatreFixture.CreateTheatre(name: "Test Theatre");

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;
        using var context = new AppDbContext(options);
        context.Theatres.Add(theatre);
        await context.SaveChangesAsync();

        // Act 1 - создать мюзикл

        var musical = _musicalFixture.CreateMusical(
            title: "Test Musical",
            description: "Test Description",
            duration: TimeSpan.FromHours(2.5),
            ageRestriction: AgeRestriction.SixteenPlus,
            theatreId: theatre.Id);

        await repo.AddAsync(musical);

        // Assert 1 - проверить создание
        var created = await repo.GetByIdAsync(musical.Id);
        created.Should().NotBeNull();
        created!.Title.Should().Be("Test Musical");
        created.AgeRestriction.Should().Be(AgeRestriction.SixteenPlus);
        created.TheatreId.Should().Be(theatre.Id);

        // Act 2 - обновить мюзикл
        created.Title = "Updated Musical";
        created.AgeRestriction = AgeRestriction.EighteenPlus;
        await repo.UpdateAsync(created);

        // Assert 2 - проверить обновление
        var updated = await repo.GetByIdAsync(musical.Id);
        updated!.Title.Should().Be("Updated Musical");
        updated.AgeRestriction.Should().Be(AgeRestriction.EighteenPlus);

        // Act 3 - получить мюзиклы по театру
        var theatreMusicals = await repo.GetByTheatreIdAsync(theatre.Id);

        // Assert 3 - проверить фильтрацию по театру
        theatreMusicals.Should().ContainSingle(m => m.Id == musical.Id);

        // Act 4 - получить мюзиклы по возрастному ограничению
        var adultMusicals = await repo.GetByAgeRestrictionAsync(AgeRestriction.EighteenPlus);

        // Assert 4 - проверить фильтрацию по возрастному ограничению
        adultMusicals.Should().ContainSingle(m => m.Id == musical.Id);

        // Act 5 - удалить мюзикл
        await repo.RemoveAsync(updated);

        // Assert 5 - проверить удаление
        var deleted = await repo.GetByIdAsync(musical.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task Musical_GetByAgeRestriction_ReturnsCorrectMusicals()
    {
        // Arrange
        var repo = CreateRepository();

        var theatre = _theatreFixture.CreateTheatre();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;
        using var context = new AppDbContext(options);
        context.Theatres.Add(theatre);
        await context.SaveChangesAsync();

        var musical1 = _musicalFixture.CreateMusical(
            title: "Musical 16+",
            ageRestriction: AgeRestriction.SixteenPlus,
            theatreId: theatre.Id);

        var musical2 = _musicalFixture.CreateMusical(
            title: "Musical 18+",
            ageRestriction: AgeRestriction.EighteenPlus,
            theatreId: theatre.Id);

        await repo.AddAsync(musical1);
        await repo.AddAsync(musical2);

        // Act
        var sixteenPlus = await repo.GetByAgeRestrictionAsync(AgeRestriction.SixteenPlus);
        var eighteenPlus = await repo.GetByAgeRestrictionAsync(AgeRestriction.EighteenPlus);

        // Assert
        sixteenPlus.Should().ContainSingle(m => m.Title == "Musical 16+");
        eighteenPlus.Should().ContainSingle(m => m.Title == "Musical 18+");

        // Cleanup
        await repo.RemoveAsync(musical1);
        await repo.RemoveAsync(musical2);
    }
}