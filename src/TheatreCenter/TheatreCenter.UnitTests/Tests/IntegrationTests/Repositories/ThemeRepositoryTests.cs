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
public class ThemeRepositoryIt(DatabaseFixture db, ThemeFixture themeFixture)
    : IClassFixture<DatabaseFixture>, IClassFixture<ThemeFixture>
{
    private ThemeRepository CreateRepository()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;

        var context = new AppDbContext(options);
        return new ThemeRepository(context);
    }

    [Fact]
    public async Task Theme_FullCycle()
    {
        // Arrange
        var repo = CreateRepository();

        // Act 1 - создать тему
        var theme = themeFixture.CreateTheme(name: "Test Theme 123");

        await repo.AddAsync(theme);

        // Assert 1 - проверить создание
        var created = await repo.GetByIdAsync(theme.Id);
        created.Should().NotBeNull();
        created!.Name.Should().Be("Test Theme 123");

        // Act 2 - обновить тему
        created.Name = "Updated Theme";
        await repo.UpdateAsync(created);

        // Assert 2 - проверить обновление
        var updated = await repo.GetByIdAsync(theme.Id);
        updated!.Name.Should().Be("Updated Theme");

        // Act 3 - получить все темы
        var allThemes = await repo.GetAllAsync();

        // Act 4 - удалить тему
        await repo.RemoveAsync(updated);

        // Assert 4 - проверить удаление
        var deleted = await repo.GetByIdAsync(theme.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task Theme_GetAll_ReturnsAllThemes()
    {
        // Arrange
        var repo = CreateRepository();

        var theme1 = themeFixture.CreateTheme(name: "Theme 1");
        var theme2 = themeFixture.CreateTheme(name: "Theme 2");

        await repo.AddAsync(theme1);
        await repo.AddAsync(theme2);

        // Act
        var allThemes = await repo.GetAllAsync();

        // Assert
        allThemes.Should().HaveCountGreaterThanOrEqualTo(2);
        allThemes.Should().Contain(t => t.Name == "Theme 1");
        allThemes.Should().Contain(t => t.Name == "Theme 2");

        // Cleanup
        await repo.RemoveAsync(theme1);
        await repo.RemoveAsync(theme2);
    }
}