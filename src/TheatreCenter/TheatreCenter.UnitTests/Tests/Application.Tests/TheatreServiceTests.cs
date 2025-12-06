using Allure.Xunit.Attributes;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TheatreCenter.Domain.Interfaces.Repositories;
using TheatreCenter.Domain.Models;
using TheatreCenter.Services.Services;
using TheatreCenter.Tests.Fixtures;
using TheatreCenter.UnitTests;

namespace TheatreCenter.Tests.Services;

[AllureSuite("Theatre Service Tests")]
[AllureSubSuite("London Style (with Mocks)")]
[Trait("Category", TestCategories.Unit)]
public class TheatreServiceMockTests : IClassFixture<TheatreFixture>
{
    private readonly Mock<ITheatreRepository> _theatreRepositoryMock;
    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly Mock<ILogger<TheatreService>> _loggerMock;
    private readonly TheatreService _sut;
    private readonly TheatreFixture _fixture;

    public TheatreServiceMockTests(TheatreFixture fixture)
    {
        _fixture = fixture;
        _theatreRepositoryMock = new Mock<ITheatreRepository>();
        _accountRepositoryMock = new Mock<IAccountRepository>();
        _loggerMock = new Mock<ILogger<TheatreService>>();
        _sut = new TheatreService(_theatreRepositoryMock.Object, _accountRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    [AllureFeature("GetTheatreByIdAsync")]
    [AllureStory("Positive case - theatre exists")]
    public async Task GetTheatreByIdAsync_TheatreExists_ReturnsTheatre()
    {

        var theatreId = 1;
        var expectedTheatre = _fixture.CreateTheatre(id: theatreId);

        _theatreRepositoryMock
            .Setup(repo => repo.GetByIdAsync(theatreId))
            .ReturnsAsync(expectedTheatre);


        var result = await _sut.GetTheatreByIdAsync(theatreId);


        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedTheatre);
        _theatreRepositoryMock.Verify(repo => repo.GetByIdAsync(theatreId), Times.Once);
    }

    [Fact]
    [AllureFeature("GetTheatreByIdAsync")]
    [AllureStory("Positive case - theatre exists with current user")]
    public async Task GetTheatreByIdAsync_TheatreExistsWithCurrentUser_ReturnsTheatre()
    {

        var theatreId = 1;
        var userId = 100;
        var expectedTheatre = _fixture.CreateTheatre(id: theatreId);
        var isFavorite = true;

        _theatreRepositoryMock
            .Setup(repo => repo.GetByIdAsync(theatreId))
            .ReturnsAsync(expectedTheatre);

        _accountRepositoryMock
            .Setup(repo => repo.IsFavoriteTheatreAsync(userId, theatreId))
            .ReturnsAsync(isFavorite);


        var result = await _sut.GetTheatreByIdAsync(theatreId, userId);


        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedTheatre);
        result!.IsFavorite.Should().Be(isFavorite);
        _theatreRepositoryMock.Verify(repo => repo.GetByIdAsync(theatreId), Times.Once);
        _accountRepositoryMock.Verify(repo => repo.IsFavoriteTheatreAsync(userId, theatreId), Times.Once);
    }

    [Fact]
    [AllureFeature("GetTheatreByIdAsync")]
    [AllureStory("Negative case - theatre not found")]
    public async Task GetTheatreByIdAsync_TheatreNotExists_ThrowsKeyNotFoundException()
    {

        var theatreId = 1;

        _theatreRepositoryMock
            .Setup(repo => repo.GetByIdAsync(theatreId))
            .ReturnsAsync((Theatre?)null);


        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetTheatreByIdAsync(theatreId));
        _theatreRepositoryMock.Verify(repo => repo.GetByIdAsync(theatreId), Times.Once);
    }

    [Fact]
    [AllureFeature("GetTheatreByIdAsync")]
    [AllureStory("Exception handling")]
    public async Task GetTheatreByIdAsync_RepositoryThrowsException_ThrowsException()
    {

        var theatreId = 1;
        var exception = new Exception("Database error");

        _theatreRepositoryMock
            .Setup(repo => repo.GetByIdAsync(theatreId))
            .ThrowsAsync(exception);


        await Assert.ThrowsAsync<Exception>(() => _sut.GetTheatreByIdAsync(theatreId));
        _theatreRepositoryMock.Verify(repo => repo.GetByIdAsync(theatreId), Times.Once);
    }

    [Fact]
    [AllureFeature("GetAllTheatresAsync")]
    [AllureStory("Positive case - theatres exist")]
    public async Task GetAllTheatresAsync_TheatresExist_ReturnsTheatres()
    {
        var filter = new TheatreFilter();
        var theatres = new List<Theatre>
        {
            _fixture.CreateTheatre(id: 1, name: "Theatre 1"),
            _fixture.CreateTheatre(id: 2, name: "Theatre 2")
        };

        _theatreRepositoryMock
            .Setup(repo => repo.GetAllAsync(filter))
            .ReturnsAsync(theatres);


        var result = await _sut.GetAllTheatresAsync(filter);


        result.Should().HaveCount(2);
        _theatreRepositoryMock.Verify(repo => repo.GetAllAsync(filter), Times.Once);
    }

    [Fact]
    [AllureFeature("GetAllTheatresAsync")]
    [AllureStory("Positive case - theatres exist with current user")]
    public async Task GetAllTheatresAsync_TheatresExistWithCurrentUser_ReturnsTheatres()
    {
        var userId = 100;
        var filter = new TheatreFilter();
        var theatres = new List<Theatre>
        {
            _fixture.CreateTheatre(id: 1, name: "Theatre 1"),
            _fixture.CreateTheatre(id: 2, name: "Theatre 2")
        };

        _theatreRepositoryMock
            .Setup(repo => repo.GetAllAsync(filter))
            .ReturnsAsync(theatres);

        _accountRepositoryMock
            .Setup(repo => repo.IsFavoriteTheatreAsync(userId, It.IsAny<int>()))
            .ReturnsAsync(true);


        var result = await _sut.GetAllTheatresAsync(filter, userId);


        result.Should().HaveCount(2);
        result.Should().Match<List<Theatre>>(list => list.All(t => t.IsFavorite == true));
        _theatreRepositoryMock.Verify(repo => repo.GetAllAsync(filter), Times.Once);
        _accountRepositoryMock.Verify(repo => repo.IsFavoriteTheatreAsync(userId, It.IsAny<int>()), Times.Exactly(2));
    }

    [Fact]
    [AllureFeature("GetAllTheatresAsync")]
    [AllureStory("Negative case - no theatres")]
    public async Task GetAllTheatresAsync_NoTheatres_ReturnsEmptyList()
    {
        var filter = new TheatreFilter();

        _theatreRepositoryMock
            .Setup(repo => repo.GetAllAsync(filter))
            .ReturnsAsync(new List<Theatre>());


        var result = await _sut.GetAllTheatresAsync(filter);


        result.Should().BeEmpty();
        _theatreRepositoryMock.Verify(repo => repo.GetAllAsync(filter), Times.Once);
    }

    [Fact]
    [AllureFeature("GetAllTheatresAsync")]
    [AllureStory("Exception handling")]
    public async Task GetAllTheatresAsync_RepositoryThrowsException_ThrowsException()
    {
        var filter = new TheatreFilter();
        var exception = new Exception("Database error");

        _theatreRepositoryMock
            .Setup(repo => repo.GetAllAsync(filter))
            .ThrowsAsync(exception);


        await Assert.ThrowsAsync<Exception>(() => _sut.GetAllTheatresAsync(filter));
        _theatreRepositoryMock.Verify(repo => repo.GetAllAsync(filter), Times.Once);
    }

    [Fact]
    [AllureFeature("GetCountAsync")]
    [AllureStory("Positive case - count returned")]
    public async Task GetCountAsync_TheatresExist_ReturnsCount()
    {
        var filter = new TheatreFilter();
        var expectedCount = 5;

        _theatreRepositoryMock
            .Setup(repo => repo.GetCountAsync(filter))
            .ReturnsAsync(expectedCount);


        var result = await _sut.GetCountAsync(filter);


        result.Should().Be(expectedCount);
        _theatreRepositoryMock.Verify(repo => repo.GetCountAsync(filter), Times.Once);
    }

    [Fact]
    [AllureFeature("GetCountAsync")]
    [AllureStory("Exception handling")]
    public async Task GetCountAsync_RepositoryThrowsException_ThrowsException()
    {
        var filter = new TheatreFilter();
        var exception = new Exception("Database error");

        _theatreRepositoryMock
            .Setup(repo => repo.GetCountAsync(filter))
            .ThrowsAsync(exception);


        await Assert.ThrowsAsync<Exception>(() => _sut.GetCountAsync(filter));
        _theatreRepositoryMock.Verify(repo => repo.GetCountAsync(filter), Times.Once);
    }

    [Fact]
    [AllureFeature("CreateTheatreAsync")]
    [AllureStory("Positive case - theatre created")]
    public async Task CreateTheatreAsync_ValidTheatre_ReturnsCreatedTheatre()
    {

        var theatre = _fixture.CreateTheatre(id: 1, name: "New Theatre");

        _theatreRepositoryMock
            .Setup(repo => repo.AddAsync(theatre))
            .Returns(Task.CompletedTask);
        _theatreRepositoryMock
            .Setup(repo => repo.SaveChangesAsync())
            .Returns(Task.CompletedTask);


        var result = await _sut.CreateTheatreAsync(theatre);


        result.Should().BeEquivalentTo(theatre);
        _theatreRepositoryMock.Verify(repo => repo.AddAsync(theatre), Times.Once);
        _theatreRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    [AllureFeature("CreateTheatreAsync")]
    [AllureStory("Negative case - null theatre")]
    public async Task CreateTheatreAsync_NullTheatre_ThrowsArgumentNullException()
    {

        Theatre theatre = null!;


        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.CreateTheatreAsync(theatre));
        _theatreRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Theatre>()), Times.Never);
        _theatreRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    [AllureFeature("CreateTheatreAsync")]
    [AllureStory("Exception handling")]
    public async Task CreateTheatreAsync_RepositoryThrowsException_ThrowsException()
    {

        var theatre = _fixture.CreateTheatre();
        var exception = new Exception("Create failed");

        _theatreRepositoryMock
            .Setup(repo => repo.AddAsync(theatre))
            .ThrowsAsync(exception);


        await Assert.ThrowsAsync<Exception>(() => _sut.CreateTheatreAsync(theatre));
        _theatreRepositoryMock.Verify(repo => repo.AddAsync(theatre), Times.Once);
        _theatreRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    [AllureFeature("UpdateTheatreAsync")]
    [AllureStory("Positive case - theatre updated")]
    public async Task UpdateTheatreAsync_ValidTheatre_ReturnsUpdatedTheatre()
    {

        var theatre = _fixture.CreateTheatre(id: 1, name: "Updated Theatre");
        var existingTheatre = _fixture.CreateTheatre(id: 1, name: "Original Theatre");

        _theatreRepositoryMock
            .Setup(repo => repo.GetByIdAsync(theatre.Id))
            .ReturnsAsync(existingTheatre);
        _theatreRepositoryMock
            .Setup(repo => repo.UpdateAsync(existingTheatre))
            .Returns(Task.CompletedTask);
        _theatreRepositoryMock
            .Setup(repo => repo.SaveChangesAsync())
            .Returns(Task.CompletedTask);


        var result = await _sut.UpdateTheatreAsync(theatre);


        result.Should().BeEquivalentTo(theatre);
        _theatreRepositoryMock.Verify(repo => repo.GetByIdAsync(theatre.Id), Times.Once);
        _theatreRepositoryMock.Verify(repo => repo.UpdateAsync(existingTheatre), Times.Once);
        _theatreRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    [AllureFeature("UpdateTheatreAsync")]
    [AllureStory("Negative case - null theatre")]
    public async Task UpdateTheatreAsync_NullTheatre_ThrowsArgumentNullException()
    {

        Theatre theatre = null!;


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

        var theatre = _fixture.CreateTheatre(id: 1);

        _theatreRepositoryMock
            .Setup(repo => repo.GetByIdAsync(theatre.Id))
            .ReturnsAsync((Theatre?)null);


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

        var theatre = _fixture.CreateTheatre(id: 1);
        var existingTheatre = _fixture.CreateTheatre(id: 1);
        var exception = new Exception("Update failed");

        _theatreRepositoryMock
            .Setup(repo => repo.GetByIdAsync(theatre.Id))
            .ReturnsAsync(existingTheatre);
        _theatreRepositoryMock
            .Setup(repo => repo.UpdateAsync(existingTheatre))
            .ThrowsAsync(exception);


        await Assert.ThrowsAsync<Exception>(() => _sut.UpdateTheatreAsync(theatre));
        _theatreRepositoryMock.Verify(repo => repo.GetByIdAsync(theatre.Id), Times.Once);
        _theatreRepositoryMock.Verify(repo => repo.UpdateAsync(existingTheatre), Times.Once);
        _theatreRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    [AllureFeature("DeleteTheatreAsync")]
    [AllureStory("Positive case - theatre deleted")]
    public async Task DeleteTheatreAsync_ValidId_ReturnsTrue()
    {

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


        var result = await _sut.DeleteTheatreAsync(theatreId);


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

        var theatreId = 1;

        _theatreRepositoryMock
            .Setup(repo => repo.GetByIdAsync(theatreId))
            .ReturnsAsync((Theatre?)null);


        var result = await _sut.DeleteTheatreAsync(theatreId);


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

        var theatreId = 1;
        var existingTheatre = _fixture.CreateTheatre(id: theatreId);
        var exception = new Exception("Delete failed");

        _theatreRepositoryMock
            .Setup(repo => repo.GetByIdAsync(theatreId))
            .ReturnsAsync(existingTheatre);
        _theatreRepositoryMock
            .Setup(repo => repo.RemoveAsync(existingTheatre))
            .ThrowsAsync(exception);


        await Assert.ThrowsAsync<Exception>(() => _sut.DeleteTheatreAsync(theatreId));
        _theatreRepositoryMock.Verify(repo => repo.GetByIdAsync(theatreId), Times.Once);
        _theatreRepositoryMock.Verify(repo => repo.RemoveAsync(existingTheatre), Times.Once);
        _theatreRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }
}
