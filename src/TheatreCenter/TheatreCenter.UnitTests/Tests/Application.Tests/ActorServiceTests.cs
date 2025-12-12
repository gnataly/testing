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

[AllureSuite("Actor Service Tests")]
[AllureSubSuite("London Style (with Mocks)")]
[Trait("Category", TestCategories.Unit)]
public class ActorServiceMockTests : IClassFixture<ActorFixture>
{
    private readonly Mock<IActorRepository> _actorRepositoryMock;
    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly Mock<ILogger<ActorService>> _loggerMock;
    private readonly ActorService _sut;
    private readonly ActorFixture _fixture;

    public ActorServiceMockTests(ActorFixture fixture)
    {
        _fixture = fixture;
        _actorRepositoryMock = new Mock<IActorRepository>();
        _accountRepositoryMock = new Mock<IAccountRepository>();
        _loggerMock = new Mock<ILogger<ActorService>>();
        _sut = new ActorService(_actorRepositoryMock.Object, _accountRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    [AllureFeature("GetActorByIdAsync")]
    [AllureStory("Positive case - actor exists")]
    public async Task GetActorByIdAsync_ActorExists_ReturnsActor()
    {

        var actorId = 1;
        var expectedActor = _fixture.CreateActor(id: actorId);

        _actorRepositoryMock
            .Setup(repo => repo.GetByIdAsync(actorId))
            .ReturnsAsync(expectedActor);


        var result = await _sut.GetActorByIdAsync(actorId);


        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedActor);
        _actorRepositoryMock.Verify(repo => repo.GetByIdAsync(actorId), Times.Once);

        // Verify logging
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Attempting to get actor")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    [AllureFeature("GetActorByIdAsync")]
    [AllureStory("Positive case - actor exists with current user")]
    public async Task GetActorByIdAsync_ActorExistsWithCurrentUser_ReturnsActor()
    {

        var actorId = 1;
        var userId = 100;
        var expectedActor = _fixture.CreateActor(id: actorId);
        var isFavorite = true;

        _actorRepositoryMock
            .Setup(repo => repo.GetByIdAsync(actorId))
            .ReturnsAsync(expectedActor);

        _accountRepositoryMock
            .Setup(repo => repo.IsFavoriteActorAsync(userId, actorId))
            .ReturnsAsync(isFavorite);


        var result = await _sut.GetActorByIdAsync(actorId, userId);


        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedActor);
        result!.IsFavorite.Should().Be(isFavorite);
        _actorRepositoryMock.Verify(repo => repo.GetByIdAsync(actorId), Times.Once);
        _accountRepositoryMock.Verify(repo => repo.IsFavoriteActorAsync(userId, actorId), Times.Once);

        // Verify logging
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Attempting to get actor")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    [AllureFeature("GetActorByIdAsync")]
    [AllureStory("Negative case - invalid ID")]
    public async Task GetActorByIdAsync_InvalidId_ThrowsArgumentException()
    {

        var invalidId = 0;


        await Assert.ThrowsAsync<ArgumentException>(() => _sut.GetActorByIdAsync(invalidId));
        _actorRepositoryMock.Verify(repo => repo.GetByIdAsync(It.IsAny<int>()), Times.Never);

        // Verify logging
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid actor ID")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    [AllureFeature("GetActorByIdAsync")]
    [AllureStory("Negative case - actor not found")]
    public async Task GetActorByIdAsync_ActorNotFound_ThrowsKeyNotFoundException()
    {

        var actorId = 1;

        _actorRepositoryMock
            .Setup(repo => repo.GetByIdAsync(actorId))
            .ReturnsAsync((Actor?)null);


        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetActorByIdAsync(actorId));
        _actorRepositoryMock.Verify(repo => repo.GetByIdAsync(actorId), Times.Once);

        // Verify logging
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    [AllureFeature("GetAllActorsAsync")]
    [AllureStory("Positive case - actors exist")]
    public async Task GetAllActorsAsync_ActorsExist_ReturnsActors()
    {
        var filter = new ActorFilter();
        var actors = new List<Actor>
        {
            _fixture.CreateActor(id: 1),
            _fixture.CreateActor(id: 2)
        };

        _actorRepositoryMock
            .Setup(repo => repo.GetAllAsync(filter))
            .ReturnsAsync(actors);


        var result = await _sut.GetAllActorsAsync(filter);


        result.Should().HaveCount(2);
        _actorRepositoryMock.Verify(repo => repo.GetAllAsync(filter), Times.Once);

        // Verify logging
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("retrieve all actors")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    [AllureFeature("GetAllActorsAsync")]
    [AllureStory("Positive case - actors exist with current user")]
    public async Task GetAllActorsAsync_ActorsExistWithCurrentUser_ReturnsActors()
    {
        var userId = 100;
        var filter = new ActorFilter();
        var actors = new List<Actor>
        {
            _fixture.CreateActor(id: 1),
            _fixture.CreateActor(id: 2)
        };

        _actorRepositoryMock
            .Setup(repo => repo.GetAllAsync(filter))
            .ReturnsAsync(actors);

        _accountRepositoryMock
            .Setup(repo => repo.IsFavoriteActorAsync(userId, It.IsAny<int>()))
            .ReturnsAsync(true);


        var result = await _sut.GetAllActorsAsync(filter, userId);


        result.Should().HaveCount(2);
        result.Should().Match<List<Actor>>(list => list.All(a => a.IsFavorite == true));
        _actorRepositoryMock.Verify(repo => repo.GetAllAsync(filter), Times.Once);
        _accountRepositoryMock.Verify(repo => repo.IsFavoriteActorAsync(userId, It.IsAny<int>()), Times.Exactly(2));

        // Verify logging
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("retrieve all actors")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    [AllureFeature("GetCountAsync")]
    [AllureStory("Positive case - count returned")]
    public async Task GetCountAsync_ActorsExist_ReturnsCount()
    {
        var filter = new ActorFilter();
        var expectedCount = 5;

        _actorRepositoryMock
            .Setup(repo => repo.GetCountAsync(filter))
            .ReturnsAsync(expectedCount);


        var result = await _sut.GetCountAsync(filter);


        result.Should().Be(expectedCount);
        _actorRepositoryMock.Verify(repo => repo.GetCountAsync(filter), Times.Once);

        // Verify logging
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("retrieve all actors")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    [AllureFeature("CreateActorAsync")]
    [AllureStory("Positive case - valid actor")]
    public async Task CreateActorAsync_ValidActor_ReturnsCreatedActor()
    {

        var actor = _fixture.CreateActor();

        _actorRepositoryMock
            .Setup(repo => repo.AddAsync(actor))
            .Returns(Task.CompletedTask);
        _actorRepositoryMock
            .Setup(repo => repo.SaveChangesAsync())
            .Returns(Task.CompletedTask);


        var result = await _sut.CreateActorAsync(actor);


        result.Should().BeEquivalentTo(actor);
        _actorRepositoryMock.Verify(repo => repo.AddAsync(actor), Times.Once);
        _actorRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);

        // Verify logging
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully created")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    [AllureFeature("CreateActorAsync")]
    [AllureStory("Negative case - null actor")]
    public async Task CreateActorAsync_NullActor_ThrowsArgumentNullException()
    {

        Actor actor = null!;


        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.CreateActorAsync(actor));
        _actorRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Actor>()), Times.Never);

        // Verify logging
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Attempted to create null actor")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    [AllureFeature("CreateActorAsync")]
    [AllureStory("Negative case - empty name")]
    public async Task CreateActorAsync_EmptyName_ThrowsArgumentException()
    {

        var actor = _fixture.CreateActor();
        actor.Name = "";


        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateActorAsync(actor));
        _actorRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Actor>()), Times.Never);

        // Verify logging
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("empty name")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    [AllureFeature("CreateActorAsync")]
    [AllureStory("Negative case - future birth date")]
    public async Task CreateActorAsync_FutureBirthDate_ThrowsArgumentException()
    {

        var actor = _fixture.CreateActor();
        actor.BirthDate = DateTime.UtcNow.AddDays(1);


        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateActorAsync(actor));
        _actorRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Actor>()), Times.Never);

        // Verify logging
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid birth date")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    [AllureFeature("CreateActorAsync")]
    [AllureStory("Negative case - invalid height")]
    public async Task CreateActorAsync_InvalidHeight_ThrowsArgumentException()
    {

        var actor = _fixture.CreateActor();
        actor.Height = 50; // Too short


        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateActorAsync(actor));
        _actorRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Actor>()), Times.Never);

        // Verify logging
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid height value")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    [AllureFeature("UpdateActorAsync")]
    [AllureStory("Positive case - valid update")]
    public async Task UpdateActorAsync_ValidActor_ReturnsUpdatedActor()
    {

        var actor = _fixture.CreateActor(id: 1);
        var existingActor = _fixture.CreateActor(id: 1);

        _actorRepositoryMock
            .Setup(repo => repo.GetByIdAsync(actor.Id))
            .ReturnsAsync(existingActor);
        _actorRepositoryMock
            .Setup(repo => repo.UpdateAsync(existingActor))
            .Returns(Task.CompletedTask);
        _actorRepositoryMock
            .Setup(repo => repo.SaveChangesAsync())
            .Returns(Task.CompletedTask);


        var result = await _sut.UpdateActorAsync(actor);


        result.Should().BeEquivalentTo(existingActor);
        _actorRepositoryMock.Verify(repo => repo.GetByIdAsync(actor.Id), Times.Once);
        _actorRepositoryMock.Verify(repo => repo.UpdateAsync(existingActor), Times.Once);
        _actorRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);

        // Verify logging
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully updated")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    [AllureFeature("DeleteActorAsync")]
    [AllureStory("Positive case - actor deleted")]
    public async Task DeleteActorAsync_ValidActor_ReturnsTrue()
    {

        var actorId = 1;
        var actor = _fixture.CreateActor(id: actorId);
        actor.ActorRoles = new List<ActorRole>(); // No assigned roles

        _actorRepositoryMock
            .Setup(repo => repo.GetByIdAsync(actorId))
            .ReturnsAsync(actor);
        _actorRepositoryMock
            .Setup(repo => repo.RemoveAsync(actor))
            .Returns(Task.CompletedTask);
        _actorRepositoryMock
            .Setup(repo => repo.SaveChangesAsync())
            .Returns(Task.CompletedTask);


        var result = await _sut.DeleteActorAsync(actorId);


        result.Should().BeTrue();
        _actorRepositoryMock.Verify(repo => repo.GetByIdAsync(actorId), Times.Once);
        _actorRepositoryMock.Verify(repo => repo.RemoveAsync(actor), Times.Once);
        _actorRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);

        // Verify logging
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully deleted")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    [AllureFeature("DeleteActorAsync")]
    [AllureStory("Negative case - actor with assigned roles")]
    public async Task DeleteActorAsync_ActorWithRoles_ThrowsInvalidOperationException()
    {

        var actorId = 1;
        var actor = _fixture.CreateActor(id: actorId);
        actor.ActorRoles = new List<ActorRole> { new ActorRole(1, 1) }; // Has assigned roles

        _actorRepositoryMock
            .Setup(repo => repo.GetByIdAsync(actorId))
            .ReturnsAsync(actor);


        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.DeleteActorAsync(actorId));
        _actorRepositoryMock.Verify(repo => repo.RemoveAsync(It.IsAny<Actor>()), Times.Never);

        // Verify logging
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("has assigned roles")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
