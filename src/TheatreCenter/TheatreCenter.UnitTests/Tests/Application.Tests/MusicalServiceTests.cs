using Allure.Xunit.Attributes;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TheatreCenter.Domain.Enums;
using TheatreCenter.Domain.Interfaces.Repositories;
using TheatreCenter.Domain.Models;
using TheatreCenter.Services.Services;
using TheatreCenter.Tests.Fixtures;
using TheatreCenter.UnitTests;
using Xunit;

namespace TheatreCenter.Tests.Services;

[AllureSuite("Musical Service Tests")]
[AllureSubSuite("London Style (with Mocks)")]
[Trait("Category", TestCategories.Unit)]
public class MusicalServiceMockTests : IClassFixture<MusicalFixture>
{
    private readonly Mock<IMusicalRepository> _musicalRepositoryMock;
    private readonly Mock<ITheatreRepository> _theatreRepositoryMock;
    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly Mock<ILogger<MusicalService>> _loggerMock;
    private readonly MusicalService _sut;
    private readonly MusicalFixture _fixture;

    public MusicalServiceMockTests(MusicalFixture fixture)
    {
        _fixture = fixture;
        _musicalRepositoryMock = new Mock<IMusicalRepository>();
        _theatreRepositoryMock = new Mock<ITheatreRepository>();
        _accountRepositoryMock = new Mock<IAccountRepository>();
        _loggerMock = new Mock<ILogger<MusicalService>>();
        _sut = new MusicalService(_musicalRepositoryMock.Object, _theatreRepositoryMock.Object, _accountRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    [AllureFeature("GetMusicalByIdAsync")]
    [AllureStory("Positive case - musical exists")]
    public async Task GetMusicalByIdAsync_MusicalExists_ReturnsMusical()
    {

        var musicalId = 1;
        var expectedMusical = _fixture.CreateMusical(id: musicalId);

        _musicalRepositoryMock
            .Setup(repo => repo.GetByIdAsync(musicalId))
            .ReturnsAsync(expectedMusical);


        var result = await _sut.GetMusicalByIdAsync(musicalId);


        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedMusical);
        _musicalRepositoryMock.Verify(repo => repo.GetByIdAsync(musicalId), Times.Once);
    }

    [Fact]
    [AllureFeature("GetMusicalByIdAsync")]
    [AllureStory("Positive case - musical exists with current user")]
    public async Task GetMusicalByIdAsync_MusicalExistsWithCurrentUser_ReturnsMusical()
    {

        var musicalId = 1;
        var userId = 100;
        var expectedMusical = _fixture.CreateMusical(id: musicalId);
        var isFavorite = true;

        _musicalRepositoryMock
            .Setup(repo => repo.GetByIdAsync(musicalId))
            .ReturnsAsync(expectedMusical);

        _accountRepositoryMock
            .Setup(repo => repo.IsFavoriteMusicalAsync(userId, musicalId))
            .ReturnsAsync(isFavorite);


        var result = await _sut.GetMusicalByIdAsync(musicalId, userId);


        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedMusical);
        result!.IsFavorite.Should().Be(isFavorite);
        _musicalRepositoryMock.Verify(repo => repo.GetByIdAsync(musicalId), Times.Once);
        _accountRepositoryMock.Verify(repo => repo.IsFavoriteMusicalAsync(userId, musicalId), Times.Once);
    }

    [Fact]
    [AllureFeature("GetMusicalByIdAsync")]
    [AllureStory("Negative case - invalid ID")]
    public async Task GetMusicalByIdAsync_InvalidId_ThrowsArgumentException()
    {
        
        var invalidId = 0;

        
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.GetMusicalByIdAsync(invalidId));
        _musicalRepositoryMock.Verify(repo => repo.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    [AllureFeature("GetMusicalByIdAsync")]
    [AllureStory("Negative case - musical not found")]
    public async Task GetMusicalByIdAsync_MusicalNotFound_ThrowsKeyNotFoundException()
    {
        
        var musicalId = 1;

        _musicalRepositoryMock
            .Setup(repo => repo.GetByIdAsync(musicalId))
            .ReturnsAsync((Musical?)null);

        
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetMusicalByIdAsync(musicalId));
        _musicalRepositoryMock.Verify(repo => repo.GetByIdAsync(musicalId), Times.Once);
    }

    [Fact]
    [AllureFeature("CreateMusicalAsync")]
    [AllureStory("Positive case - valid musical")]
    public async Task CreateMusicalAsync_ValidMusical_ReturnsCreatedMusical()
    {
        
        var musical = _fixture.CreateMusical();
        var theatre = _fixture.CreateTheatre(id: musical.TheatreId);

        _theatreRepositoryMock
            .Setup(repo => repo.GetByIdAsync(musical.TheatreId))
            .ReturnsAsync(theatre);
        _musicalRepositoryMock
            .Setup(repo => repo.AddAsync(musical))
            .Returns(Task.CompletedTask);
        _musicalRepositoryMock
            .Setup(repo => repo.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        
        var result = await _sut.CreateMusicalAsync(musical);

        
        result.Should().BeEquivalentTo(musical);
        _theatreRepositoryMock.Verify(repo => repo.GetByIdAsync(musical.TheatreId), Times.Once);
        _musicalRepositoryMock.Verify(repo => repo.AddAsync(musical), Times.Once);
        _musicalRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    [AllureFeature("CreateMusicalAsync")]
    [AllureStory("Negative case - empty title")]
    public async Task CreateMusicalAsync_EmptyTitle_ThrowsArgumentException()
    {
        
        var musical = _fixture.CreateMusical();
        musical.Title = "";

        
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateMusicalAsync(musical));
        _musicalRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Musical>()), Times.Never);
    }

    [Fact]
    [AllureFeature("CreateMusicalAsync")]
    [AllureStory("Negative case - invalid duration")]
    public async Task CreateMusicalAsync_InvalidDuration_ThrowsArgumentException()
    {
        
        var musical = _fixture.CreateMusical();
        musical.Duration = TimeSpan.Zero;

        
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateMusicalAsync(musical));
        _musicalRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Musical>()), Times.Never);
    }

    [Fact]
    [AllureFeature("CreateMusicalAsync")]
    [AllureStory("Negative case - theatre not found")]
    public async Task CreateMusicalAsync_TheatreNotFound_ThrowsArgumentException()
    {
        
        var musical = _fixture.CreateMusical();

        _theatreRepositoryMock
            .Setup(repo => repo.GetByIdAsync(musical.TheatreId))
            .ReturnsAsync((Theatre?)null);

        
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateMusicalAsync(musical));
        _musicalRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Musical>()), Times.Never);
    }

    [Fact]
    [AllureFeature("DeleteMusicalAsync")]
    [AllureStory("Positive case - musical deleted")]
    public async Task DeleteMusicalAsync_ValidMusical_ReturnsTrue()
    {
        
        var musicalId = 1;
        var musical = _fixture.CreateMusical(id: musicalId);
        musical.Shows = new List<Show>(); // No scheduled shows

        _musicalRepositoryMock
            .Setup(repo => repo.GetByIdAsync(musicalId))
            .ReturnsAsync(musical);
        _musicalRepositoryMock
            .Setup(repo => repo.RemoveAsync(musical))
            .Returns(Task.CompletedTask);
        _musicalRepositoryMock
            .Setup(repo => repo.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        
        var result = await _sut.DeleteMusicalAsync(musicalId);

        
        result.Should().BeTrue();
        _musicalRepositoryMock.Verify(repo => repo.GetByIdAsync(musicalId), Times.Once);
        _musicalRepositoryMock.Verify(repo => repo.RemoveAsync(musical), Times.Once);
        _musicalRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    [AllureFeature("DeleteMusicalAsync")]
    [AllureStory("Negative case - musical with scheduled shows")]
    public async Task DeleteMusicalAsync_MusicalWithShows_ThrowsInvalidOperationException()
    {
        
        var musicalId = 1;
        var musical = _fixture.CreateMusical(id: musicalId);
        musical.Shows = new List<Show> { new Show(1, DateTime.UtcNow, 1) };

        _musicalRepositoryMock
            .Setup(repo => repo.GetByIdAsync(musicalId))
            .ReturnsAsync(musical);

        
        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.DeleteMusicalAsync(musicalId));
        _musicalRepositoryMock.Verify(repo => repo.RemoveAsync(It.IsAny<Musical>()), Times.Never);
    }

    [Fact]
    [AllureFeature("GetAllMusicalsAsync")]
    [AllureStory("Positive case - musicals exist")]
    public async Task GetAllMusicalsAsync_MusicalsExist_ReturnsMusicals()
    {
        var filter = new MusicalFilter();
        var musicals = new List<Musical>
        {
            _fixture.CreateMusical(),
            _fixture.CreateMusical()
        };

        _musicalRepositoryMock
            .Setup(repo => repo.GetAllAsync(filter))
            .ReturnsAsync(musicals);


        var result = await _sut.GetAllMusicalsAsync(filter);


        result.Should().HaveCount(2);
        _musicalRepositoryMock.Verify(repo => repo.GetAllAsync(filter), Times.Once);
    }

    [Fact]
    [AllureFeature("GetAllMusicalsAsync")]
    [AllureStory("Positive case - musicals exist with current user")]
    public async Task GetAllMusicalsAsync_MusicalsExistWithCurrentUser_ReturnsMusicals()
    {
        var userId = 100;
        var filter = new MusicalFilter();
        var musicals = new List<Musical>
        {
            _fixture.CreateMusical(),
            _fixture.CreateMusical()
        };

        _musicalRepositoryMock
            .Setup(repo => repo.GetAllAsync(filter))
            .ReturnsAsync(musicals);

        _accountRepositoryMock
            .Setup(repo => repo.IsFavoriteMusicalAsync(userId, It.IsAny<int>()))
            .ReturnsAsync(true);


        var result = await _sut.GetAllMusicalsAsync(filter, userId);


        result.Should().HaveCount(2);
        result.Should().Match<List<Musical>>(list => list.All(m => m.IsFavorite == true));
        _musicalRepositoryMock.Verify(repo => repo.GetAllAsync(filter), Times.Once);
        _accountRepositoryMock.Verify(repo => repo.IsFavoriteMusicalAsync(userId, It.IsAny<int>()), Times.Exactly(2));
    }

    [Fact]
    [AllureFeature("GetCountAsync")]
    [AllureStory("Positive case - count returned")]
    public async Task GetCountAsync_MusicalsExist_ReturnsCount()
    {
        var filter = new MusicalFilter();
        var expectedCount = 5;

        _musicalRepositoryMock
            .Setup(repo => repo.GetCountAsync(filter))
            .ReturnsAsync(expectedCount);


        var result = await _sut.GetCountAsync(filter);


        result.Should().Be(expectedCount);
        _musicalRepositoryMock.Verify(repo => repo.GetCountAsync(filter), Times.Once);
    }
}