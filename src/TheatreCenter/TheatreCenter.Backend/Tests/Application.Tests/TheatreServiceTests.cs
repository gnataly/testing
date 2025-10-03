using Allure.Xunit.Attributes;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TheatreCenter.Domain.Interfaces.Repositories;
using TheatreCenter.Domain.Models;
using TheatreCenter.Services.Services;
using TheatreCenter.Tests.Fixtures;

namespace TheatreCenter.Tests.Services;

[AllureSuite("Theatre Service Tests")]
[AllureSubSuite("London Style (with Mocks)")]
public class TheatreServiceMockTests : IClassFixture<TheatreFixture>
{
    private readonly Mock<ITheatreRepository> _theatreRepositoryMock;
    private readonly Mock<ILogger<TheatreService>> _loggerMock;
    private readonly TheatreService _sut;
    private readonly TheatreFixture _fixture;

    public TheatreServiceMockTests(TheatreFixture fixture)
    {
        _fixture = fixture;
        _theatreRepositoryMock = new Mock<ITheatreRepository>();
        _loggerMock = new Mock<ILogger<TheatreService>>();
        _sut = new TheatreService(_theatreRepositoryMock.Object);
    }

    [Fact]
    [AllureFeature("GetTheatreByIdAsync")]
    [AllureStory("Positive case - theatre exists")]
    public async Task GetTheatreByIdAsync_TheatreExists_ReturnsTheatre()
    {
        // Arrange
        var theatreId = 1;
        var expectedTheatre = _fixture.CreateTheatre(id: theatreId);

        _theatreRepositoryMock
            .Setup(repo => repo.GetByIdAsync(theatreId))
            .ReturnsAsync(expectedTheatre);

        // Act
        var result = await _sut.GetTheatreByIdAsync(theatreId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedTheatre);
        _theatreRepositoryMock.Verify(repo => repo.GetByIdAsync(theatreId), Times.Once);
    }

    [Fact]
    [AllureFeature("GetTheatreByIdAsync")]
    [AllureStory("Negative case - theatre not found")]
    public async Task GetTheatreByIdAsync_TheatreNotExists_ReturnsNull()
    {
        // Arrange
        var theatreId = 1;

        _theatreRepositoryMock
            .Setup(repo => repo.GetByIdAsync(theatreId))
            .ReturnsAsync((Theatre?)null);

        // Act
        var result = await _sut.GetTheatreByIdAsync(theatreId);

        // Assert
        result.Should().BeNull();
        _theatreRepositoryMock.Verify(repo => repo.GetByIdAsync(theatreId), Times.Once);
    }

    [Fact]
    [AllureFeature("GetTheatreByIdAsync")]
    [AllureStory("Exception handling")]
    public async Task GetTheatreByIdAsync_RepositoryThrowsException_ThrowsException()
    {
        // Arrange
        var theatreId = 1;
        var exception = new Exception("Database error");

        _theatreRepositoryMock
            .Setup(repo => repo.GetByIdAsync(theatreId))
            .ThrowsAsync(exception);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _sut.GetTheatreByIdAsync(theatreId));
        _theatreRepositoryMock.Verify(repo => repo.GetByIdAsync(theatreId), Times.Once);
    }

    [Fact]
    [AllureFeature("GetAllTheatresAsync")]
    [AllureStory("Positive case - theatres exist")]
    public async Task GetAllTheatresAsync_TheatresExist_ReturnsTheatres()
    {
        // Arrange
        var theatres = new List<Theatre>
        {
            _fixture.CreateTheatre(id: 1, name: "Theatre 1"),
            _fixture.CreateTheatre(id: 2, name: "Theatre 2")
        };

        _theatreRepositoryMock
            .Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(theatres);

        // Act
        var result = await _sut.GetAllTheatresAsync();

        // Assert
        result.Should().HaveCount(2);
        _theatreRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
    }

    [Fact]
    [AllureFeature("GetAllTheatresAsync")]
    [AllureStory("Negative case - no theatres")]
    public async Task GetAllTheatresAsync_NoTheatres_ReturnsEmptyList()
    {
        // Arrange
        _theatreRepositoryMock
            .Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(new List<Theatre>());

        // Act
        var result = await _sut.GetAllTheatresAsync();

        // Assert
        result.Should().BeEmpty();
        _theatreRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
    }

    [Fact]
    [AllureFeature("GetAllTheatresAsync")]
    [AllureStory("Exception handling")]
    public async Task GetAllTheatresAsync_RepositoryThrowsException_ThrowsException()
    {
        // Arrange
        var exception = new Exception("Database error");

        _theatreRepositoryMock
            .Setup(repo => repo.GetAllAsync())
            .ThrowsAsync(exception);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _sut.GetAllTheatresAsync());
        _theatreRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
    }

    [Fact]
    [AllureFeature("CreateTheatreAsync")]
    [AllureStory("Positive case - theatre created")]
    public async Task CreateTheatreAsync_ValidTheatre_ReturnsCreatedTheatre()
    {
        // Arrange
        var theatre = _fixture.CreateTheatre(id: 1, name: "New Theatre");

        _theatreRepositoryMock
            .Setup(repo => repo.AddAsync(theatre))
            .Returns(Task.CompletedTask);
        _theatreRepositoryMock
            .Setup(repo => repo.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateTheatreAsync(theatre);

        // Assert
        result.Should().BeEquivalentTo(theatre);
        _theatreRepositoryMock.Verify(repo => repo.AddAsync(theatre), Times.Once);
        _theatreRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    [AllureFeature("CreateTheatreAsync")]
    [AllureStory("Negative case - null theatre")]
    public async Task CreateTheatreAsync_NullTheatre_ThrowsArgumentNullException()
    {
        // Arrange
        Theatre theatre = null!;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.CreateTheatreAsync(theatre));
        _theatreRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Theatre>()), Times.Never);
        _theatreRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    [AllureFeature("CreateTheatreAsync")]
    [AllureStory("Exception handling")]
    public async Task CreateTheatreAsync_RepositoryThrowsException_ThrowsException()
    {
        // Arrange
        var theatre = _fixture.CreateTheatre();
        var exception = new Exception("Create failed");

        _theatreRepositoryMock
            .Setup(repo => repo.AddAsync(theatre))
            .ThrowsAsync(exception);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _sut.CreateTheatreAsync(theatre));
        _theatreRepositoryMock.Verify(repo => repo.AddAsync(theatre), Times.Once);
        _theatreRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    [AllureFeature("UpdateTheatreAsync")]
    [AllureStory("Positive case - theatre updated")]
    public async Task UpdateTheatreAsync_ValidTheatre_ReturnsUpdatedTheatre()
    {
        // Arrange
        var theatre = _fixture.CreateTheatre(id: 1, name: "Updated Theatre");
        var existingTheatre = _fixture.CreateTheatre(id: 1, name: "Original Theatre");

        _theatreRepositoryMock
            .Setup(repo => repo.GetByIdAsync(theatre.Id))
            .ReturnsAsync(existingTheatre);
        _theatreRepositoryMock
            .Setup(repo => repo.UpdateAsync(theatre))
            .Returns(Task.CompletedTask);
        _theatreRepositoryMock
            .Setup(repo => repo.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateTheatreAsync(theatre);

        // Assert
        result.Should().BeEquivalentTo(theatre);
        _theatreRepositoryMock.Verify(repo => repo.GetByIdAsync(theatre.Id), Times.Once);
        _theatreRepositoryMock.Verify(repo => repo.UpdateAsync(theatre), Times.Once);
        _theatreRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    [AllureFeature("UpdateTheatreAsync")]
    [AllureStory("Negative case - null theatre")]
    public async Task UpdateTheatreAsync_NullTheatre_ThrowsArgumentNullException()
    {
        // Arrange
        Theatre theatre = null!;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.UpdateTheatreAsync(theatre));
        _theatreRepositoryMock.Verify(repo => repo.GetByIdAsync(It.IsAny<int>()), Times.Never);
        _theatreRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Theatre>()), Times.Never);
        _theatreRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    [AllureFeature("UpdateTheatreAsync")]
    [AllureStory("Negative case - theatre not found")]
    public async Task UpdateTheatreAsync_TheatreNotExists_ThrowsKeyNotFoundException()
    {
        // Arrange
        var theatre = _fixture.CreateTheatre(id: 1);

        _theatreRepositoryMock
            .Setup(repo => repo.GetByIdAsync(theatre.Id))
            .ReturnsAsync((Theatre?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.UpdateTheatreAsync(theatre));
        _theatreRepositoryMock.Verify(repo => repo.GetByIdAsync(theatre.Id), Times.Once);
        _theatreRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Theatre>()), Times.Never);
        _theatreRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    [AllureFeature("UpdateTheatreAsync")]
    [AllureStory("Exception handling")]
    public async Task UpdateTheatreAsync_RepositoryThrowsException_ThrowsException()
    {
        // Arrange
        var theatre = _fixture.CreateTheatre(id: 1);
        var existingTheatre = _fixture.CreateTheatre(id: 1);
        var exception = new Exception("Update failed");

        _theatreRepositoryMock
            .Setup(repo => repo.GetByIdAsync(theatre.Id))
            .ReturnsAsync(existingTheatre);
        _theatreRepositoryMock
            .Setup(repo => repo.UpdateAsync(theatre))
            .ThrowsAsync(exception);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _sut.UpdateTheatreAsync(theatre));
        _theatreRepositoryMock.Verify(repo => repo.GetByIdAsync(theatre.Id), Times.Once);
        _theatreRepositoryMock.Verify(repo => repo.UpdateAsync(theatre), Times.Once);
        _theatreRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    [AllureFeature("DeleteTheatreAsync")]
    [AllureStory("Positive case - theatre deleted")]
    public async Task DeleteTheatreAsync_ValidId_ReturnsTrue()
    {
        // Arrange
        var theatreId = 1;
        var existingTheatre = _fixture.CreateTheatre(id: theatreId);

        _theatreRepositoryMock
            .Setup(repo => repo.GetByIdAsync(theatreId))
            .ReturnsAsync(existingTheatre);
        _theatreRepositoryMock
            .Setup(repo => repo.RemoveAsync(existingTheatre))
            .Returns(Task.CompletedTask);
        _theatreRepositoryMock
            .Setup(repo => repo.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.DeleteTheatreAsync(theatreId);

        // Assert
        result.Should().BeTrue();
        _theatreRepositoryMock.Verify(repo => repo.GetByIdAsync(theatreId), Times.Once);
        _theatreRepositoryMock.Verify(repo => repo.RemoveAsync(existingTheatre), Times.Once);
        _theatreRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    [AllureFeature("DeleteTheatreAsync")]
    [AllureStory("Negative case - theatre not found")]
    public async Task DeleteTheatreAsync_TheatreNotExists_ReturnsFalse()
    {
        // Arrange
        var theatreId = 1;

        _theatreRepositoryMock
            .Setup(repo => repo.GetByIdAsync(theatreId))
            .ReturnsAsync((Theatre?)null);

        // Act
        var result = await _sut.DeleteTheatreAsync(theatreId);

        // Assert
        result.Should().BeFalse();
        _theatreRepositoryMock.Verify(repo => repo.GetByIdAsync(theatreId), Times.Once);
        _theatreRepositoryMock.Verify(repo => repo.RemoveAsync(It.IsAny<Theatre>()), Times.Never);
        _theatreRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    [AllureFeature("DeleteTheatreAsync")]
    [AllureStory("Exception handling")]
    public async Task DeleteTheatreAsync_RepositoryThrowsException_ThrowsException()
    {
        // Arrange
        var theatreId = 1;
        var existingTheatre = _fixture.CreateTheatre(id: theatreId);
        var exception = new Exception("Delete failed");

        _theatreRepositoryMock
            .Setup(repo => repo.GetByIdAsync(theatreId))
            .ReturnsAsync(existingTheatre);
        _theatreRepositoryMock
            .Setup(repo => repo.RemoveAsync(existingTheatre))
            .ThrowsAsync(exception);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _sut.DeleteTheatreAsync(theatreId));
        _theatreRepositoryMock.Verify(repo => repo.GetByIdAsync(theatreId), Times.Once);
        _theatreRepositoryMock.Verify(repo => repo.RemoveAsync(existingTheatre), Times.Once);
        _theatreRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }
}