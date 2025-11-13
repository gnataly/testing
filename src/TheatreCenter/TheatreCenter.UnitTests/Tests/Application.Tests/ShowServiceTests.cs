using Allure.Xunit.Attributes;
using FluentAssertions;
using Moq;
using TheatreCenter.Domain.Interfaces.Repositories;
using TheatreCenter.Domain.Models;
using TheatreCenter.Services.Services;
using TheatreCenter.Tests.Fixtures;
using TheatreCenter.UnitTests;
using Xunit;

namespace TheatreCenter.Tests.Services;

[AllureSuite("Show Service Tests")]
[AllureSubSuite("London Style (with Mocks)")]
[Trait("Category", TestCategories.Unit)]
public class ShowServiceMockTests : IClassFixture<ShowFixture>
{
    private readonly Mock<IShowRepository> _showRepositoryMock;
    private readonly Mock<IMusicalRepository> _musicalRepositoryMock;
    private readonly ShowService _sut;
    private readonly ShowFixture _fixture;

    public ShowServiceMockTests(ShowFixture fixture)
    {
        _fixture = fixture;
        _showRepositoryMock = new Mock<IShowRepository>();
        _musicalRepositoryMock = new Mock<IMusicalRepository>();
        _sut = new ShowService(_showRepositoryMock.Object, _musicalRepositoryMock.Object);
    }

    [Fact]
    [AllureFeature("GetByIdAsync")]
    [AllureStory("Positive case - show exists")]
    public async Task GetByIdAsync_ShowExists_ReturnsShow()
    {
        
        var showId = 1;
        var expectedShow = _fixture.CreateShow(id: showId);

        _showRepositoryMock
            .Setup(repo => repo.GetByIdAsync(showId))
            .ReturnsAsync(expectedShow);

        
        var result = await _sut.GetByIdAsync(showId);

        
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedShow);
        _showRepositoryMock.Verify(repo => repo.GetByIdAsync(showId), Times.Once);
    }

    [Fact]
    [AllureFeature("GetByIdAsync")]
    [AllureStory("Negative case - invalid ID")]
    public async Task GetByIdAsync_InvalidId_ThrowsArgumentException()
    {
        
        var invalidId = 0;

        
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.GetByIdAsync(invalidId));
        _showRepositoryMock.Verify(repo => repo.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    [AllureFeature("GetByIdAsync")]
    [AllureStory("Negative case - show not found")]
    public async Task GetByIdAsync_ShowNotFound_ThrowsKeyNotFoundException()
    {
        
        var showId = 1;

        _showRepositoryMock
            .Setup(repo => repo.GetByIdAsync(showId))
            .ReturnsAsync((Show?)null);

        
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetByIdAsync(showId));
        _showRepositoryMock.Verify(repo => repo.GetByIdAsync(showId), Times.Once);
    }

    [Fact]
    [AllureFeature("CreateAsync")]
    [AllureStory("Positive case - valid show")]
    public async Task CreateAsync_ValidShow_ReturnsCreatedShow()
    {
        
        var show = _fixture.CreateShow(futureDate: true);
        var musical = _fixture.CreateMusical(id: show.MusicalId);

        _musicalRepositoryMock
            .Setup(repo => repo.GetByIdAsync(show.MusicalId))
            .ReturnsAsync(musical);
        _showRepositoryMock
            .Setup(repo => repo.AddAsync(show))
            .Returns(Task.CompletedTask);
        _showRepositoryMock
            .Setup(repo => repo.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        
        var result = await _sut.CreateAsync(show);

        
        result.Should().BeEquivalentTo(show);
        _musicalRepositoryMock.Verify(repo => repo.GetByIdAsync(show.MusicalId), Times.Once);
        _showRepositoryMock.Verify(repo => repo.AddAsync(show), Times.Once);
        _showRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    [AllureFeature("CreateAsync")]
    [AllureStory("Negative case - past date")]
    public async Task CreateAsync_PastDate_ThrowsArgumentException()
    {
        
        var show = _fixture.CreateShow(futureDate: false); // Past date

        
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateAsync(show));
        _showRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Show>()), Times.Never);
    }

    [Fact]
    [AllureFeature("CreateAsync")]
    [AllureStory("Negative case - musical not found")]
    public async Task CreateAsync_MusicalNotFound_ThrowsArgumentException()
    {
        
        var show = _fixture.CreateShow(futureDate: true);

        _musicalRepositoryMock
            .Setup(repo => repo.GetByIdAsync(show.MusicalId))
            .ReturnsAsync((Musical?)null);

        
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateAsync(show));
        _showRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Show>()), Times.Never);
    }

    [Fact]
    [AllureFeature("UpdateAsync")]
    [AllureStory("Positive case - valid update")]
    public async Task UpdateAsync_ValidShow_ReturnsUpdatedShow()
    {
        
        var show = _fixture.CreateShow(futureDate: true);
        var existingShow = _fixture.CreateShow(id: show.Id, futureDate: true);
        var musical = _fixture.CreateMusical(id: show.MusicalId);

        _showRepositoryMock
            .Setup(repo => repo.GetByIdAsync(show.Id))
            .ReturnsAsync(existingShow);
        _musicalRepositoryMock
            .Setup(repo => repo.GetByIdAsync(show.MusicalId))
            .ReturnsAsync(musical);
        _showRepositoryMock
            .Setup(repo => repo.UpdateAsync(existingShow))
            .Returns(Task.CompletedTask);
        _showRepositoryMock
            .Setup(repo => repo.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        
        var result = await _sut.UpdateAsync(show);

        
        result.Should().BeEquivalentTo(existingShow);
        _showRepositoryMock.Verify(repo => repo.GetByIdAsync(show.Id), Times.Once);
        _musicalRepositoryMock.Verify(repo => repo.GetByIdAsync(show.MusicalId), Times.Once);
        _showRepositoryMock.Verify(repo => repo.UpdateAsync(existingShow), Times.Once);
        _showRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    [AllureFeature("DeleteAsync")]
    [AllureStory("Positive case - future show deleted")]
    public async Task DeleteAsync_FutureShow_ReturnsTrue()
    {
        
        var showId = 1;
        var show = _fixture.CreateShow(id: showId, futureDate: true);

        _showRepositoryMock
            .Setup(repo => repo.GetByIdAsync(showId))
            .ReturnsAsync(show);
        _showRepositoryMock
            .Setup(repo => repo.RemoveAsync(show))
            .Returns(Task.CompletedTask);
        _showRepositoryMock
            .Setup(repo => repo.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        
        var result = await _sut.DeleteAsync(showId);

        
        result.Should().BeTrue();
        _showRepositoryMock.Verify(repo => repo.GetByIdAsync(showId), Times.Once);
        _showRepositoryMock.Verify(repo => repo.RemoveAsync(show), Times.Once);
        _showRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    [AllureFeature("DeleteAsync")]
    [AllureStory("Negative case - past show")]
    public async Task DeleteAsync_PastShow_ThrowsInvalidOperationException()
    {
        
        var showId = 1;
        var show = _fixture.CreateShow(id: showId, futureDate: false);

        _showRepositoryMock
            .Setup(repo => repo.GetByIdAsync(showId))
            .ReturnsAsync(show);

        
        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.DeleteAsync(showId));
        _showRepositoryMock.Verify(repo => repo.RemoveAsync(It.IsAny<Show>()), Times.Never);
    }

    [Fact]
    [AllureFeature("GetByMusicalIdAsync")]
    [AllureStory("Positive case - shows found")]
    public async Task GetByMusicalIdAsync_ValidId_ReturnsShows()
    {
        
        var musicalId = 1;
        var shows = new List<Show>
        {
            _fixture.CreateShow(musicalId: musicalId),
            _fixture.CreateShow(musicalId: musicalId)
        };

        _showRepositoryMock
            .Setup(repo => repo.GetByMusicalIdAsync(musicalId))
            .ReturnsAsync(shows);

        
        var result = await _sut.GetByMusicalIdAsync(musicalId);

        
        result.Should().HaveCount(2);
        _showRepositoryMock.Verify(repo => repo.GetByMusicalIdAsync(musicalId), Times.Once);
    }

    [Fact]
    [AllureFeature("GetByMusicalIdAsync")]
    [AllureStory("Negative case - invalid musical ID")]
    public async Task GetByMusicalIdAsync_InvalidId_ThrowsArgumentException()
    {
        
        var invalidId = 0;

        
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.GetByMusicalIdAsync(invalidId));
        _showRepositoryMock.Verify(repo => repo.GetByMusicalIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    [AllureFeature("GetUpcomingShowsAsync")]
    [AllureStory("Positive case - upcoming shows")]
    public async Task GetUpcomingShowsAsync_ReturnsShows()
    {
        
        var shows = new List<Show>
        {
            _fixture.CreateShow(futureDate: true),
            _fixture.CreateShow(futureDate: true)
        };

        _showRepositoryMock
            .Setup(repo => repo.GetUpcomingShowsAsync())
            .ReturnsAsync(shows);

        
        var result = await _sut.GetUpcomingShowsAsync();

        
        result.Should().HaveCount(2);
        _showRepositoryMock.Verify(repo => repo.GetUpcomingShowsAsync(), Times.Once);
    }
}