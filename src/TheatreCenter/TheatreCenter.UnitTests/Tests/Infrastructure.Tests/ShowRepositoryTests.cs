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

[AllureSuite("Show Repository Tests")]
[Trait("Category", TestCategories.Unit)]
public class ShowRepositoryTests : IClassFixture<ShowFixture>
{
    private readonly ShowFixture _fixture;

    public ShowRepositoryTests(ShowFixture fixture)
    {
        _fixture = fixture;
    }

    private ShowRepository GetInMemoryRepository()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);
        return new ShowRepository(context);
    }

    [Fact]
    [AllureFeature("GetByIdAsync")]
    [AllureStory("Positive case - show exists")]
    public async Task GetByIdAsync_ShowExists_ReturnsShow()
    {
        
        var repository = GetInMemoryRepository();
        var show = _fixture.CreateShow();

        await repository.AddAsync(show);
         

        
        var result = await repository.GetByIdAsync(show.Id);

        
        result.Should().NotBeNull();
        result.Id.Should().Be(show.Id);
        result.MusicalId.Should().Be(show.MusicalId);
    }

    [Fact]
    [AllureFeature("GetByIdAsync")]
    [AllureStory("Negative case - show not found")]
    public async Task GetByIdAsync_ShowNotExists_ReturnsNull()
    {
        
        var repository = GetInMemoryRepository();

        
        var result = await repository.GetByIdAsync(999);

        
        result.Should().BeNull();
    }

    [Fact]
    [AllureFeature("GetAllAsync")]
    [AllureStory("Positive case - returns all shows")]
    public async Task GetAllAsync_ShowsExist_ReturnsShows()
    {
        
        var repository = GetInMemoryRepository();
        var show1 = _fixture.CreateShow();
        var show2 = _fixture.CreateShow();

        await repository.AddAsync(show1);
        await repository.AddAsync(show2);
         

        
        var result = await repository.GetAllAsync();

        
        result.Should().HaveCount(2);
    }

    [Fact]
    [AllureFeature("GetAllAsync")]
    [AllureStory("Negative case - no shows")]
    public async Task GetAllAsync_NoShows_ReturnsEmpty()
    {
        
        var repository = GetInMemoryRepository();

        
        var result = await repository.GetAllAsync();

        
        result.Should().BeEmpty();
    }

    [Fact]
    [AllureFeature("AddAsync")]
    [AllureStory("Positive case - adds show")]
    public async Task AddAsync_ValidShow_AddsToDatabase()
    {
        
        var repository = GetInMemoryRepository();
        var show = _fixture.CreateShow();

        
        await repository.AddAsync(show);
         

        
        var result = await repository.GetByIdAsync(show.Id);
        result.Should().NotBeNull();
    }

    [Fact]
    [AllureFeature("UpdateAsync")]
    [AllureStory("Positive case - updates show")]
    public async Task UpdateAsync_ValidShow_UpdatesSuccessfully()
    {
        
        var repository = GetInMemoryRepository();
        var show = _fixture.CreateShow();

        await repository.AddAsync(show);
        

        var existingShow = await repository.GetByIdAsync(show.Id);
        var newDate = DateTime.UtcNow.AddDays(14);
        existingShow.Date = newDate;

        
        await repository.UpdateAsync(existingShow);

        
        var result = await repository.GetByIdAsync(show.Id);
        result.Date.Should().BeCloseTo(newDate, TimeSpan.FromSeconds(1));
    }

    [Fact]
    [AllureFeature("RemoveAsync")]
    [AllureStory("Positive case - removes show")]
    public async Task RemoveAsync_ShowExists_RemovesShow()
    {
        
        var repository = GetInMemoryRepository();
        var show = _fixture.CreateShow();

        await repository.AddAsync(show);
        

        
        await repository.RemoveAsync(show);
        

        
        var result = await repository.GetByIdAsync(show.Id);
        result.Should().BeNull();
    }

    [Fact]
    [AllureFeature("GetByMusicalIdAsync")]
    [AllureStory("Positive case - returns shows by musical")]
    public async Task GetByMusicalIdAsync_ValidMusicalId_ReturnsShows()
    {
        
        var repository = GetInMemoryRepository();
        var musicalId = 1;
        var show1 = _fixture.CreateShow(musicalId: musicalId);
        var show2 = _fixture.CreateShow(musicalId: musicalId);
        var show3 = _fixture.CreateShow(musicalId: 2);

        await repository.AddAsync(show1);
        await repository.AddAsync(show2);
        await repository.AddAsync(show3);
        

        
        var result = await repository.GetByMusicalIdAsync(musicalId);

        
        result.Should().HaveCount(2);
        result.All(s => s.MusicalId == musicalId).Should().BeTrue();
    }

    [Fact]
    [AllureFeature("GetByMusicalIdAsync")]
    [AllureStory("Negative case - no shows for musical")]
    public async Task GetByMusicalIdAsync_NoShowsForMusical_ReturnsEmpty()
    {
        
        var repository = GetInMemoryRepository();
        var show = _fixture.CreateShow(musicalId: 1);

        await repository.AddAsync(show);
        

        
        var result = await repository.GetByMusicalIdAsync(999);

        
        result.Should().BeEmpty();
    }

    [Fact]
    [AllureFeature("GetUpcomingShowsAsync")]
    [AllureStory("Positive case - returns upcoming shows")]
    public async Task GetUpcomingShowsAsync_FutureShowsExist_ReturnsShows()
    {
        
        var repository = GetInMemoryRepository();
        var futureShow1 = _fixture.CreateShow(futureDate: true);
        var futureShow2 = _fixture.CreateShow(futureDate: true);

        await repository.AddAsync(futureShow1);
        await repository.AddAsync(futureShow2);
        

        
        var result = await repository.GetUpcomingShowsAsync();

        
        result.Should().HaveCount(2);
        result.All(s => s.Date >= DateTime.UtcNow).Should().BeTrue();
    }

    [Fact]
    [AllureFeature("GetUpcomingShowsAsync")]
    [AllureStory("Negative case - no upcoming shows")]
    public async Task GetUpcomingShowsAsync_NoFutureShows_ReturnsEmpty()
    {
        
        var repository = GetInMemoryRepository();
        var pastShow = _fixture.CreateShow(futureDate: false);

        await repository.AddAsync(pastShow);
        

        
        var result = await repository.GetUpcomingShowsAsync();

        
        result.Should().BeEmpty();
    }
}