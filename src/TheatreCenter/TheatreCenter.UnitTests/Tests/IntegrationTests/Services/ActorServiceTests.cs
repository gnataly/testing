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
using Xunit.Abstractions;
using AutoFixture;
using TheatreCenter.UnitTests.Tests.IntegrationTests;

namespace TheatreCenter.Tests.IntegrationTests.Services;

[Collection("Database collection")]
[Trait("Category", TestCategories.Integration)]
public class ActorServiceIt : IntegrationTestBase
{
    private readonly ActorFixture _actorFixture = new ActorFixture();
    private ActorService _service;
    private Func<Task> _commitTransaction;
    private Func<Task> _rollbackTransaction;

    public ActorServiceIt(DatabaseFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    public override async Task InitializeAsync()
    {
        var context = await Fixture.CreateTransactionalContextAsync();
        var repository = Fixture.CreateRepository<Data.Repositories.ActorRepository>(context);
        _service = new ActorService(repository, new NullLogger<ActorService>());

        _commitTransaction = async () => {
            await context.Database.CommitTransactionAsync();
            await context.DisposeAsync();
        };
        _rollbackTransaction = async () => {
            await context.Database.RollbackTransactionAsync();
            await context.DisposeAsync();
        };
    }

    public override async Task DisposeAsync()
    {
        await _rollbackTransaction();
    }

    [Fact]
    public async Task Actor_FullCycle_WithFixtures()
    {
        // Используем фикстуру для генерации актера
        var testActor = _actorFixture.CreateActor();

        // Act 1 — создание актера
        var createdActor = await _service.CreateActorAsync(testActor);

        // Assert 1 — проверка создания
        createdActor.Should().NotBeNull();
        createdActor.Id.Should().BeGreaterThan(0);
        createdActor.Name.Should().Be(testActor.Name);

        // Act 2 — получение актера по ID
        var retrievedActor = await _service.GetActorByIdAsync(createdActor.Id);
        retrievedActor.Should().NotBeNull();
        retrievedActor.Name.Should().Be(testActor.Name);

        // Act 3 — обновление актера
        var updatedActor = _actorFixture.CreateActor(
            id: createdActor.Id,
            voiceType: VoiceType.Baritone,
            gender: Gender.Male
        );

        var updateResult = await _service.UpdateActorAsync(updatedActor);
        updateResult.Should().NotBeNull();
        updateResult.Name.Should().Be(updatedActor.Name);
        updateResult.VoiceType.Should().Be(VoiceType.Baritone);

        // Act 4 — получение актеров по типу голоса
        var actorsByVoice = await _service.GetActorsByVoiceTypeAsync(VoiceType.Baritone);
        actorsByVoice.Should().Contain(a => a.Id == createdActor.Id);

        // Act 5 — получение всех актеров
        var allActors = await _service.GetAllActorsAsync();
        allActors.Should().Contain(a => a.Id == createdActor.Id);

        // Act 6 — удаление актера
        var deleteResult = await _service.DeleteActorAsync(createdActor.Id);
        deleteResult.Should().BeTrue();
    }
}