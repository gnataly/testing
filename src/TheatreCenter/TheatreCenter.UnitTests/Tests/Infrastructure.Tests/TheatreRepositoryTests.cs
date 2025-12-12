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

[AllureSuite("Theatre Repository Tests")]
[Trait("Category", TestCategories.Unit)]
public class TheatreRepositoryTests : IClassFixture<TheatreFixture>
{
    private readonly TheatreFixture _fixture;

    public TheatreRepositoryTests(TheatreFixture fixture)
    {
        _fixture = fixture;
    }

    private TheatreRepository GetInMemoryRepository()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);
        return new TheatreRepository(context);
    }

    [Fact]
    [AllureFeature("GetByIdAsync")]
    [AllureStory("Positive case - theatre exists")]
    public async Task GetByIdAsync_TheatreExists_ReturnsTheatre()
    {

        var repository = GetInMemoryRepository();
        var theatre = _fixture.CreateTheatre();

        await repository.AddAsync(theatre);
        await repository.SaveChangesAsync();



        var result = await repository.GetByIdAsync(theatre.Id);


        result.Should().NotBeNull();
        result.Id.Should().Be(theatre.Id);
        result.Name.Should().Be(theatre.Name);
    }

    [Fact]
    [AllureFeature("GetByIdAsync")]
    [AllureStory("Negative case - theatre not found")]
    public async Task GetByIdAsync_TheatreNotExists_ReturnsNull()
    {

        var repository = GetInMemoryRepository();


        var result = await repository.GetByIdAsync(999);


        result.Should().BeNull();
    }

    [Fact]
    [AllureFeature("GetAllAsync")]
    [AllureStory("Positive case - returns all theatres")]
    public async Task GetAllAsync_TheatresExist_ReturnsTheatres()
    {

        var repository = GetInMemoryRepository();
        var theatre1 = _fixture.CreateTheatre(name: "Theatre 1");
        var theatre2 = _fixture.CreateTheatre(name: "Theatre 2");

        await repository.AddAsync(theatre1);
        await repository.AddAsync(theatre2);
        await repository.SaveChangesAsync();



        var result = await repository.GetAllAsync(new TheatreFilter());


        result.Should().HaveCount(2);
        result.Should().Contain(t => t.Name == "Theatre 1");
        result.Should().Contain(t => t.Name == "Theatre 2");
    }

    [Fact]
    [AllureFeature("GetAllAsync")]
    [AllureStory("Negative case - no theatres")]
    public async Task GetAllAsync_NoTheatres_ReturnsEmpty()
    {



        var repository = GetInMemoryRepository();


        var result = await repository.GetAllAsync(new TheatreFilter());


        result.Should().BeEmpty();
    }

    [Fact]
    [AllureFeature("AddAsync")]
    [AllureStory("Positive case - adds theatre")]
    public async Task AddAsync_ValidTheatre_AddsToDatabase()
    {

        var repository = GetInMemoryRepository();
        var theatre = _fixture.CreateTheatre();


        await repository.AddAsync(theatre);
        await repository.SaveChangesAsync();



        var result = await repository.GetByIdAsync(theatre.Id);
        result.Should().NotBeNull();
    }

    [Fact]
    [AllureFeature("UpdateAsync")]
    [AllureStory("Positive case - updates theatre")]
    public async Task UpdateAsync_ValidTheatre_UpdatesSuccessfully()
    {

        var repository = GetInMemoryRepository();
        var theatre = _fixture.CreateTheatre(name: "Original Name");

        await repository.AddAsync(theatre);
        await repository.SaveChangesAsync();


        var existingTheatre = await repository.GetByIdAsync(theatre.Id);
        existingTheatre.Name = "Updated Name";


        await repository.UpdateAsync(existingTheatre);
        await repository.SaveChangesAsync();


        var result = await repository.GetByIdAsync(theatre.Id);
        result.Name.Should().Be("Updated Name");
    }

    [Fact]
    [AllureFeature("RemoveAsync")]
    [AllureStory("Positive case - removes theatre")]
    public async Task RemoveAsync_TheatreExists_RemovesTheatre()
    {

        var repository = GetInMemoryRepository();
        var theatre = _fixture.CreateTheatre();

        await repository.AddAsync(theatre);
        await repository.SaveChangesAsync();



        await repository.RemoveAsync(theatre);
        await repository.SaveChangesAsync();


        var result = await repository.GetByIdAsync(theatre.Id);
        result.Should().BeNull();
    }
}
