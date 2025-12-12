using Allure.Xunit.Attributes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TheatreCenter.Data;
using TheatreCenter.Data.Repositories;
using TheatreCenter.Domain.Models;
using TheatreCenter.Tests.Fixtures;
using TheatreCenter.UnitTests;
using Xunit;

namespace TheatreCenter.Tests.Repositories;

[AllureSuite("Theme Repository Tests")]
[Trait("Category", TestCategories.Unit)]
public class ThemeRepositoryTests : IClassFixture<ThemeFixture>
{
    private readonly ThemeFixture _fixture;

    public ThemeRepositoryTests(ThemeFixture fixture)
    {
        _fixture = fixture;
    }

    private ThemeRepository GetInMemoryRepository()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);
        return new ThemeRepository(context);
    }

    [Fact]
    [AllureFeature("GetByIdAsync")]
    [AllureStory("Positive case - theme exists")]
    public async Task GetByIdAsync_ThemeExists_ReturnsTheme()
    {


        var repository = GetInMemoryRepository();
        var theme = _fixture.CreateTheme();

        await repository.AddAsync(theme);
        await repository.SaveChangesAsync();



        var result = await repository.GetByIdAsync(theme.Id);


        result.Should().NotBeNull();
        result.Id.Should().Be(theme.Id);
        result.Name.Should().Be(theme.Name);
    }

    [Fact]
    [AllureFeature("GetByIdAsync")]
    [AllureStory("Negative case - theme not found")]
    public async Task GetByIdAsync_ThemeNotExists_ReturnsNull()
    {

        var repository = GetInMemoryRepository();


        var result = await repository.GetByIdAsync(999);


        result.Should().BeNull();
    }

    [Fact]
    [AllureFeature("GetAllAsync")]
    [AllureStory("Positive case - returns all themes")]
    public async Task GetAllAsync_ThemesExist_ReturnsThemes()
    {

        var repository = GetInMemoryRepository();
        var theme1 = _fixture.CreateTheme(name: "Theme 1");
        var theme2 = _fixture.CreateTheme(name: "Theme 2");

        await repository.AddAsync(theme1);
        await repository.AddAsync(theme2);
        await repository.SaveChangesAsync();



        var result = await repository.GetAllAsync(new ThemeFilter());


        result.Should().HaveCount(2);
        result.Should().Contain(t => t.Name == "Theme 1");
        result.Should().Contain(t => t.Name == "Theme 2");
    }

    [Fact]
    [AllureFeature("GetAllAsync")]
    [AllureStory("Negative case - no themes")]
    public async Task GetAllAsync_NoThemes_ReturnsEmpty()
    {

        var repository = GetInMemoryRepository();


        var result = await repository.GetAllAsync(new ThemeFilter());


        result.Should().BeEmpty();
    }

    [Fact]
    [AllureFeature("AddAsync")]
    [AllureStory("Positive case - adds theme")]
    public async Task AddAsync_ValidTheme_AddsToDatabase()
    {

        var repository = GetInMemoryRepository();
        var theme = _fixture.CreateTheme();


        await repository.AddAsync(theme);
        await repository.SaveChangesAsync();



        var result = await repository.GetByIdAsync(theme.Id);
        result.Should().NotBeNull();
    }

    [Fact]
    [AllureFeature("UpdateAsync")]
    [AllureStory("Positive case - updates theme")]
    public async Task UpdateAsync_ValidTheme_UpdatesSuccessfully()
    {

        var repository = GetInMemoryRepository();
        var theme = _fixture.CreateTheme(name: "Original Name");

        await repository.AddAsync(theme);
        await repository.SaveChangesAsync();


        var existingTheme = await repository.GetByIdAsync(theme.Id);
        existingTheme.Name = "Updated Name";


        await repository.UpdateAsync(existingTheme);
        await repository.SaveChangesAsync();


        var result = await repository.GetByIdAsync(theme.Id);
        result.Name.Should().Be("Updated Name");
    }

    [Fact]
    [AllureFeature("RemoveAsync")]
    [AllureStory("Positive case - removes theme")]
    public async Task RemoveAsync_ThemeExists_RemovesTheme()
    {

        var repository = GetInMemoryRepository();
        var theme = _fixture.CreateTheme();

        await repository.AddAsync(theme);
        await repository.SaveChangesAsync();



        await repository.RemoveAsync(theme);
        await repository.SaveChangesAsync();



        var result = await repository.GetByIdAsync(theme.Id);
        result.Should().BeNull();
    }
}
