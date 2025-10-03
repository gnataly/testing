using Allure.Xunit.Attributes;
using FluentAssertions;
using Moq;
using TheatreCenter.Domain.Enums;
using TheatreCenter.Domain.Interfaces.Repositories;
using TheatreCenter.Domain.Models;
using TheatreCenter.Services.Services;
using TheatreCenter.Tests.Fixtures;
using Xunit;

namespace TheatreCenter.Tests.Services;

[AllureSuite("Musical Service Tests")]
[AllureSubSuite("London Style (with Mocks)")]
public class MusicalServiceMockTests : IClassFixture<MusicalFixture>
{
    private readonly Mock<IMusicalRepository> _musicalRepositoryMock;
    private readonly Mock<ITheatreRepository> _theatreRepositoryMock;
    private readonly MusicalService _sut;
    private readonly MusicalFixture _fixture;

    public MusicalServiceMockTests(MusicalFixture fixture)
    {
        _fixture = fixture;
        _musicalRepositoryMock = new Mock<IMusicalRepository>();
        _theatreRepositoryMock = new Mock<ITheatreRepository>();
        _sut = new MusicalService(_musicalRepositoryMock.Object, _theatreRepositoryMock.Object);
    }

    [Fact]
    [AllureFeature("GetMusicalByIdAsync")]
    [AllureStory("Positive case - musical exists")]
    public async Task GetMusicalByIdAsync_MusicalExists_ReturnsMusical()
    {
        // Arrange
        var musicalId = 1;
        var expectedMusical = _fixture.CreateMusical(id: musicalId);

        _musicalRepositoryMock
            .Setup(repo => repo.GetByIdAsync(musicalId))
            .ReturnsAsync(expectedMusical);

        // Act
        var result = await _sut.GetMusicalByIdAsync(musicalId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedMusical);
        _musicalRepositoryMock.Verify(repo => repo.GetByIdAsync(musicalId), Times.Once);
    }

    [Fact]
    [AllureFeature("GetMusicalByIdAsync")]
    [AllureStory("Negative case - invalid ID")]
    public async Task GetMusicalByIdAsync_InvalidId_ThrowsArgumentException()
    {
        // Arrange
        var invalidId = 0;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.GetMusicalByIdAsync(invalidId));
        _musicalRepositoryMock.Verify(repo => repo.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    [AllureFeature("GetMusicalByIdAsync")]
    [AllureStory("Negative case - musical not found")]
    public async Task GetMusicalByIdAsync_MusicalNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var musicalId = 1;

        _musicalRepositoryMock
            .Setup(repo => repo.GetByIdAsync(musicalId))
            .ReturnsAsync((Musical?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetMusicalByIdAsync(musicalId));
        _musicalRepositoryMock.Verify(repo => repo.GetByIdAsync(musicalId), Times.Once);
    }

    [Fact]
    [AllureFeature("CreateMusicalAsync")]
    [AllureStory("Positive case - valid musical")]
    public async Task CreateMusicalAsync_ValidMusical_ReturnsCreatedMusical()
    {
        // Arrange
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

        // Act
        var result = await _sut.CreateMusicalAsync(musical);

        // Assert
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
        // Arrange
        var musical = _fixture.CreateMusical();
        musical.Title = "";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateMusicalAsync(musical));
        _musicalRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Musical>()), Times.Never);
    }

    [Fact]
    [AllureFeature("CreateMusicalAsync")]
    [AllureStory("Negative case - invalid duration")]
    public async Task CreateMusicalAsync_InvalidDuration_ThrowsArgumentException()
    {
        // Arrange
        var musical = _fixture.CreateMusical();
        musical.Duration = TimeSpan.Zero;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateMusicalAsync(musical));
        _musicalRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Musical>()), Times.Never);
    }

    [Fact]
    [AllureFeature("CreateMusicalAsync")]
    [AllureStory("Negative case - theatre not found")]
    public async Task CreateMusicalAsync_TheatreNotFound_ThrowsArgumentException()
    {
        // Arrange
        var musical = _fixture.CreateMusical();

        _theatreRepositoryMock
            .Setup(repo => repo.GetByIdAsync(musical.TheatreId))
            .ReturnsAsync((Theatre?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateMusicalAsync(musical));
        _musicalRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Musical>()), Times.Never);
    }

    [Fact]
    [AllureFeature("DeleteMusicalAsync")]
    [AllureStory("Positive case - musical deleted")]
    public async Task DeleteMusicalAsync_ValidMusical_ReturnsTrue()
    {
        // Arrange
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

        // Act
        var result = await _sut.DeleteMusicalAsync(musicalId);

        // Assert
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
        // Arrange
        var musicalId = 1;
        var musical = _fixture.CreateMusical(id: musicalId);
        musical.Shows = new List<Show> { new Show(1, DateTime.Now, 1) }; // Has scheduled shows

        _musicalRepositoryMock
            .Setup(repo => repo.GetByIdAsync(musicalId))
            .ReturnsAsync(musical);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.DeleteMusicalAsync(musicalId));
        _musicalRepositoryMock.Verify(repo => repo.RemoveAsync(It.IsAny<Musical>()), Times.Never);
    }

    [Fact]
    [AllureFeature("GetMusicalsByTheatreAsync")]
    [AllureStory("Positive case - musicals found")]
    public async Task GetMusicalsByTheatreAsync_ValidId_ReturnsMusicals()
    {
        // Arrange
        var theatreId = 1;
        var musicals = new List<Musical>
        {
            _fixture.CreateMusical(theatreId: theatreId),
            _fixture.CreateMusical(theatreId: theatreId)
        };

        _musicalRepositoryMock
            .Setup(repo => repo.GetByTheatreIdAsync(theatreId))
            .ReturnsAsync(musicals);

        // Act
        var result = await _sut.GetMusicalsByTheatreAsync(theatreId);

        // Assert
        result.Should().HaveCount(2);
        _musicalRepositoryMock.Verify(repo => repo.GetByTheatreIdAsync(theatreId), Times.Once);
    }

    [Fact]
    [AllureFeature("GetMusicalsByTheatreAsync")]
    [AllureStory("Negative case - invalid theatre ID")]
    public async Task GetMusicalsByTheatreAsync_InvalidId_ThrowsArgumentException()
    {
        // Arrange
        var invalidId = 0;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.GetMusicalsByTheatreAsync(invalidId));
        _musicalRepositoryMock.Verify(repo => repo.GetByTheatreIdAsync(It.IsAny<int>()), Times.Never);
    }
}