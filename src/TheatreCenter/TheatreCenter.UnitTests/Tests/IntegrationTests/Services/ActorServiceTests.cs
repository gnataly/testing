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
using TheatreCenter.Data.Repositories;

namespace TheatreCenter.Tests.IntegrationTests.Services;

[CollectionDefinition("Database collection")]
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
        //await Fixture.WaitForDatabaseReadyAsync(TimeSpan.FromSeconds(30));
        var context = await Fixture.CreateTransactionalContextAsync();
        var actorRepository = Fixture.CreateRepository<ActorRepository>(context);
        var accountRepository = Fixture.CreateRepository<AccountRepository>(context);
        _service = new ActorService(actorRepository, accountRepository, new NullLogger<ActorService>());

        _commitTransaction = async () =>
        {
            await context.Database.CommitTransactionAsync();
            await context.DisposeAsync();
        };
        _rollbackTransaction = async () =>
        {
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
        var testActor = _actorFixture.CreateActor();

        var createdActor = await _service.CreateActorAsync(testActor);

        createdActor.Should().NotBeNull();
        createdActor.Id.Should().BeGreaterThan(0);
        createdActor.Name.Should().Be(testActor.Name);

        var retrievedActor = await _service.GetActorByIdAsync(createdActor.Id);
        retrievedActor.Should().NotBeNull();
        retrievedActor.Name.Should().Be(testActor.Name);

        var updatedActor = _actorFixture.CreateActor(
            id: createdActor.Id,
            voiceType: VoiceType.Baritone,
            gender: Gender.Male
        );

        var updateResult = await _service.UpdateActorAsync(updatedActor);
        updateResult.Should().NotBeNull();
        updateResult.Name.Should().Be(updatedActor.Name);
        updateResult.VoiceType.Should().Be(VoiceType.Baritone);

        var allActors = await _service.GetAllActorsAsync(new ActorFilter(), null);
        allActors.Should().Contain(a => a.Id == createdActor.Id);

        var deleteResult = await _service.DeleteActorAsync(createdActor.Id);
        deleteResult.Should().BeTrue();
    }
}
