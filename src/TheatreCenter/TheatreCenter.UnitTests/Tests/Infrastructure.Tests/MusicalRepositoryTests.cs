using Allure.Xunit.Attributes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TheatreCenter.Data;
using TheatreCenter.Data.Repositories;
using TheatreCenter.Domain.Enums;
using TheatreCenter.Domain.Models;
using TheatreCenter.Tests.Fixtures;
using TheatreCenter.UnitTests;
using Xunit;

namespace TheatreCenter.Tests.Repositories;

[AllureSuite("Musical Repository Tests")]
[Trait("Category", TestCategories.Unit)]
public class MusicalRepositoryTests : IClassFixture<MusicalFixture>
{
    private readonly MusicalFixture _fixture;

    public MusicalRepositoryTests(MusicalFixture fixture)
    {
        _fixture = fixture;
    }

    private MusicalRepository GetInMemoryRepository()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);
        return new MusicalRepository(context);
    }

    [Fact]
    [AllureFeature("GetByIdAsync")]
    [AllureStory("Positive case - musical exists")]
    public async Task GetByIdAsync_MusicalExists_ReturnsMusical()
    {
        var contextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new AppDbContext(contextOptions);

        var theatreRepository = new TheatreRepository(context);
        var musicalRepository = new MusicalRepository(context);

        var theatre = new Theatre(1, "Test Theatre", "Test Info");
        await theatreRepository.AddAsync(theatre);
        await theatreRepository.SaveChangesAsync();

        var musical = _fixture.CreateMusical(theatreId: 1);

        await musicalRepository.AddAsync(musical);
        await musicalRepository.SaveChangesAsync();



        var result = await musicalRepository.GetByIdAsync(musical.Id);


        result.Should().NotBeNull();
        result.Id.Should().Be(musical.Id);
        result.Title.Should().Be(musical.Title);
    }

    [Fact]
    [AllureFeature("GetByIdAsync")]
    [AllureStory("Negative case - musical not found")]
    public async Task GetByIdAsync_MusicalNotExists_ReturnsNull()
    {

        var repository = GetInMemoryRepository();


        var result = await repository.GetByIdAsync(999);


        result.Should().BeNull();
    }

    [Fact]
    [AllureFeature("GetAllAsync")]
    [AllureStory("Positive case - returns all musicals")]
    public async Task GetAllAsync_MusicalsExist_ReturnsMusicals()
    {
        var contextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new AppDbContext(contextOptions);

        var theatreRepository = new TheatreRepository(context);
        var musicalRepository = new MusicalRepository(context);

        var theatre = new Theatre(1, "Test Theatre", "Test Info");
        await theatreRepository.AddAsync(theatre);
        await theatreRepository.SaveChangesAsync();

        var musical1 = _fixture.CreateMusical(title: "Musical 1", theatreId: 1);
        var musical2 = _fixture.CreateMusical(title: "Musical 2", theatreId: 1);

        await musicalRepository.AddAsync(musical1);
        await musicalRepository.AddAsync(musical2);
        await musicalRepository.SaveChangesAsync();



        var result = await musicalRepository.GetAllAsync(new MusicalFilter());


        result.Should().HaveCount(2);
        result.Should().Contain(m => m.Title == "Musical 1");
        result.Should().Contain(m => m.Title == "Musical 2");
    }

    [Fact]
    [AllureFeature("GetAllAsync")]
    [AllureStory("Negative case - no musicals")]
    public async Task GetAllAsync_NoMusicals_ReturnsEmpty()
    {

        var repository = GetInMemoryRepository();


        var result = await repository.GetAllAsync(new MusicalFilter());


        result.Should().BeEmpty();
    }

    [Fact]
    [AllureFeature("AddAsync")]
    [AllureStory("Positive case - adds musical")]
    public async Task AddAsync_ValidMusical_AddsToDatabase()
    {
        var contextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new AppDbContext(contextOptions);

        var theatreRepository = new TheatreRepository(context);
        var musicalRepository = new MusicalRepository(context);

        var theatre = new Theatre(1, "Test Theatre", "Test Info");
        await theatreRepository.AddAsync(theatre);
        await theatreRepository.SaveChangesAsync();

        var musical = _fixture.CreateMusical(theatreId: 1);

        await musicalRepository.AddAsync(musical);
        await musicalRepository.SaveChangesAsync();



        var result = await musicalRepository.GetByIdAsync(musical.Id);
        result.Should().NotBeNull();
    }

    [Fact]
    [AllureFeature("UpdateAsync")]
    [AllureStory("Positive case - updates musical")]
    public async Task UpdateAsync_ValidMusical_UpdatesSuccessfully()
    {

        var contextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new AppDbContext(contextOptions);

        var theatreRepository = new TheatreRepository(context);
        var musicalRepository = new MusicalRepository(context);

        var theatre = new Theatre(1, "Test Theatre", "Test Info");
        await theatreRepository.AddAsync(theatre);
        await theatreRepository.SaveChangesAsync();

        var musical = _fixture.CreateMusical(title: "Original Title", theatreId: 1);

        await musicalRepository.AddAsync(musical);
        await musicalRepository.SaveChangesAsync();


        var existingMusical = await musicalRepository.GetByIdAsync(musical.Id);
        existingMusical.Title = "Updated Title";


        await musicalRepository.UpdateAsync(existingMusical);
        await musicalRepository.SaveChangesAsync();


        var result = await musicalRepository.GetByIdAsync(musical.Id);
        result.Title.Should().Be("Updated Title");
    }

    [Fact]
    [AllureFeature("RemoveAsync")]
    [AllureStory("Positive case - removes musical")]
    public async Task RemoveAsync_MusicalExists_RemovesMusical()
    {

        var repository = GetInMemoryRepository();
        var musical = _fixture.CreateMusical();

        await repository.AddAsync(musical);
        await repository.SaveChangesAsync();



        await repository.RemoveAsync(musical);
        await repository.SaveChangesAsync();



        var result = await repository.GetByIdAsync(musical.Id);
        result.Should().BeNull();
    }

    [Fact]
    [AllureFeature("GetByTheatreIdAsync")]
    [AllureStory("Positive case - returns musicals by theatre")]
    public async Task GetByTheatreIdAsync_ValidTheatreId_ReturnsMusicals()
    {
        var contextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new AppDbContext(contextOptions);

        var theatreRepository = new TheatreRepository(context);
        var musicalRepository = new MusicalRepository(context);

        var theatre1 = new Theatre(1, "Test Theatre 1", "Test Info");
        var theatre2 = new Theatre(2, "Test Theatre 2", "Test Info");
        await theatreRepository.AddAsync(theatre1);
        await theatreRepository.AddAsync(theatre2);
        await theatreRepository.SaveChangesAsync();

        var musical1 = _fixture.CreateMusical(theatreId: 1, title: "Musical 1");
        var musical2 = _fixture.CreateMusical(theatreId: 1, title: "Musical 2");
        var musical3 = _fixture.CreateMusical(theatreId: 2, title: "Musical 3");

        await musicalRepository.AddAsync(musical1);
        await musicalRepository.AddAsync(musical2);
        await musicalRepository.AddAsync(musical3);
        await musicalRepository.SaveChangesAsync();



        var result = await musicalRepository.GetByTheatreIdAsync(1);


        result.Should().HaveCount(2);
        result.Should().Contain(m => m.Title == "Musical 1");
        result.Should().Contain(m => m.Title == "Musical 2");
    }

    [Fact]
    [AllureFeature("GetByTheatreIdAsync")]
    [AllureStory("Negative case - no musicals for theatre")]
    public async Task GetByTheatreIdAsync_NoMusicalsForTheatre_ReturnsEmpty()
    {

        var repository = GetInMemoryRepository();
        var musical = _fixture.CreateMusical(theatreId: 1);

        await repository.AddAsync(musical);
        await repository.SaveChangesAsync();



        var result = await repository.GetByTheatreIdAsync(999);


        result.Should().BeEmpty();
    }

    [Fact]
    [AllureFeature("GetByAgeRestrictionAsync")]
    [AllureStory("Positive case - returns musicals by age restriction")]
    public async Task GetByAgeRestrictionAsync_ValidRestriction_ReturnsMusicals()
    {
        var contextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new AppDbContext(contextOptions);

        var theatreRepository = new TheatreRepository(context);
        var musicalRepository = new MusicalRepository(context);

        var theatre = new Theatre(1, "Test Theatre", "Test Info");
        await theatreRepository.AddAsync(theatre);
        await theatreRepository.SaveChangesAsync();

        var musical1 = _fixture.CreateMusical(ageRestriction: AgeRestriction.SixteenPlus, title: "Musical 1", theatreId: 1);
        var musical2 = _fixture.CreateMusical(ageRestriction: AgeRestriction.SixteenPlus, title: "Musical 2", theatreId: 1);
        var musical3 = _fixture.CreateMusical(ageRestriction: AgeRestriction.EighteenPlus, title: "Musical 3", theatreId: 1);

        await musicalRepository.AddAsync(musical1);
        await musicalRepository.AddAsync(musical2);
        await musicalRepository.AddAsync(musical3);
        await musicalRepository.SaveChangesAsync();



        var result = await musicalRepository.GetByAgeRestrictionAsync(AgeRestriction.SixteenPlus);


        result.Should().HaveCount(2);
        result.Should().Contain(m => m.Title == "Musical 1");
        result.Should().Contain(m => m.Title == "Musical 2");
    }

    [Fact]
    [AllureFeature("GetByAgeRestrictionAsync")]
    [AllureStory("Negative case - no musicals with age restriction")]
    public async Task GetByAgeRestrictionAsync_NoMusicalsWithRestriction_ReturnsEmpty()
    {

        var repository = GetInMemoryRepository();
        var musical = _fixture.CreateMusical(ageRestriction: AgeRestriction.SixteenPlus);

        await repository.AddAsync(musical);
        await repository.SaveChangesAsync();



        var result = await repository.GetByAgeRestrictionAsync(AgeRestriction.EighteenPlus);


        result.Should().BeEmpty();
    }
}