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
public class TheatreRepositoryIt(DatabaseFixture db) : IClassFixture<DatabaseFixture>
{
    private readonly TheatreFixture _theatreFixture = new TheatreFixture();
    private TheatreRepository CreateRepository()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;

        var context = new AppDbContext(options);
        return new TheatreRepository(context);
    }

    [Fact]
    public async Task Theatre_FullCycle_WithMusicals()
    {
        // Arrange
        var repo = CreateRepository();

        // Act 1 - создать театр
        var theatre = _theatreFixture.CreateTheatre(
            name: "Test Theatre",
            addInfo: "Test information");

        await repo.AddAsync(theatre);

        // Assert 1 - проверить создание
        var created = await repo.GetByIdAsync(theatre.Id);
        created.Should().NotBeNull();
        created!.Name.Should().Be("Test Theatre");
        created.AddInfo.Should().Be("Test information");

        // Act 2 - обновить театр
        created.Name = "Updated Theatre";
        created.AddInfo = "Updated information";
        await repo.UpdateAsync(created);

        // Assert 2 - проверить обновление
        var updated = await repo.GetByIdAsync(theatre.Id);
        updated!.Name.Should().Be("Updated Theatre");
        updated.AddInfo.Should().Be("Updated information");

        // Act 3 - получить все театры
        var allTheatres = await repo.GetAllAsync();

        // Assert 3 - проверить получение всех театров
        allTheatres.Should().Contain(t => t.Id == theatre.Id);

        // Act 4 - удалить театр
        await repo.RemoveAsync(updated);

        // Assert 4 - проверить удаление
        var deleted = await repo.GetByIdAsync(theatre.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task Theatre_GetAll_ReturnsAllTheatres()
    {
        // Arrange
        var repo = CreateRepository();

        var theatre1 = _theatreFixture.CreateTheatre(name: "Theatre 1");
        var theatre2 = _theatreFixture.CreateTheatre(name: "Theatre 2");

        await repo.AddAsync(theatre1);
        await repo.AddAsync(theatre2);

        // Act
        var allTheatres = await repo.GetAllAsync();

        // Assert
        allTheatres.Should().HaveCountGreaterThanOrEqualTo(2);
        allTheatres.Should().Contain(t => t.Name == "Theatre 1");
        allTheatres.Should().Contain(t => t.Name == "Theatre 2");

        // Cleanup
        await repo.RemoveAsync(theatre1);
        await repo.RemoveAsync(theatre2);
    }
}