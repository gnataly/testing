using TheatreCenter.Data;
using TheatreCenter.Domain.Models;
using TheatreCenter.Domain.Enums;
using TheatreCenter.Services.Services;
using TheatreCenter.Tests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TheatreCenter.UnitTests.Tests.Database;
using TheatreCenter.UnitTests;
using Xunit;

namespace TheatreCenter.Tests.IntegrationTests.Services;

[Collection("Database collection")]
[Trait("Category", TestCategories.Integration)]
public class ActorServiceIt(DatabaseFixture db) : IClassFixture<DatabaseFixture>
{
    private readonly ActorFixture _actorFixture = new ActorFixture();

    private ActorService CreateService()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;

        var context = new AppDbContext(options);
        var repository = new ActorRepository(context, new NullLogger<ActorRepository>());
        return new ActorService(repository, new NullLogger<ActorService>());
    }

    [Fact]
    public async Task Actor_FullCycle_WithFixtures()
    {
        var service = CreateService();

        // Используем фикстуру для генерации актера
        var testActor = _actorFixture.CreateActor(
            name: "Test Actor"
        );

        // Act 1 — создание актера
        var createdActor = await service.CreateActorAsync(testActor);

        // Assert 1 — проверка создания
        createdActor.Should().NotBeNull();
        createdActor.Id.Should().BeGreaterThan(0);
        createdActor.Name.Should().Be(testActor.Name);

        // Act 2 — получение актера по ID
        var retrievedActor = await service.GetActorByIdAsync(createdActor.Id);
        retrievedActor.Should().NotBeNull();
        retrievedActor.Name.Should().Be(testActor.Name);

        // Act 3 — обновление актера
        var updatedActor = _actorFixture.CreateActor(
            id: createdActor.Id,
            name: "Updated Actor Name",
            voiceType: VoiceType.Baritone,
            gender: Gender.Male
        );

        var updateResult = await service.UpdateActorAsync(updatedActor);
        updateResult.Should().NotBeNull();
        updateResult.Name.Should().Be("Updated Actor Name");
        updateResult.VoiceType.Should().Be(VoiceType.Baritone);

        // Act 4 — получение актеров по типу голоса
        var actorsByVoice = await service.GetActorsByVoiceTypeAsync(VoiceType.Baritone);
        actorsByVoice.Should().Contain(a => a.Id == createdActor.Id);

        // Act 5 — получение всех актеров
        var allActors = await service.GetAllActorsAsync();
        allActors.Should().Contain(a => a.Id == createdActor.Id);

        // Act 6 — удаление актера
        var deleteResult = await service.DeleteActorAsync(createdActor.Id);
        deleteResult.Should().BeTrue();

        //// Assert 6 — проверка удаления
        //var deletedActor = await service.GetActorByIdAsync(createdActor.Id);
        //deletedActor.Should().BeNull();
    }
}