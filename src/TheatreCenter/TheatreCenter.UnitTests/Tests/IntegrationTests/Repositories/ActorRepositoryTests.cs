using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TheatreCenter.Data;
using TheatreCenter.Data.Repositories;
using TheatreCenter.Domain.Enums;
using TheatreCenter.Domain.Models;
using TheatreCenter.Tests.Fixtures;
using FluentAssertions;
using TheatreCenter.UnitTests.Tests.Database;
using TheatreCenter.UnitTests;
using Xunit;
using Xunit.Abstractions;
using AutoFixture;
using TheatreCenter.UnitTests.Tests.IntegrationTests;

namespace TheatreCenter.Tests.IntegrationTests.Repositories;

[CollectionDefinition("Database collection")]
[Trait("Category", TestCategories.Integration)]
public class ActorRepositoryIt : IntegrationTestBase
{
    private readonly ActorFixture _actorFixture = new ActorFixture();
    private ActorRepository _actorRepository;
    private Func<Task> _commitTransaction;
    private Func<Task> _rollbackTransaction;

    public ActorRepositoryIt(DatabaseFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    public override async Task InitializeAsync()
    {

        //await Fixture.WaitForDatabaseReadyAsync(TimeSpan.FromSeconds(30));
        //var (repository, commit, rollback) = await Fixture.CreateTransactionalRepositoryAsync<ActorRepository>();
        //_repository = repository;
        //_commitTransaction = commit;
        //_rollbackTransaction = rollback;

        var context = await Fixture.CreateTransactionalContextAsync();

        _actorRepository = Fixture.CreateRepository<ActorRepository>(context);

        _commitTransaction = async () => { await context.Database.CommitTransactionAsync(); await context.DisposeAsync(); };
        _rollbackTransaction = async () => { await context.Database.RollbackTransactionAsync(); await context.DisposeAsync(); };
    }

    public override async Task DisposeAsync()
    {
        await _rollbackTransaction();
    }

    [Fact]
    public async Task Actor_FullCycle_WithVoiceType()
    {
        var actor = _actorFixture.CreateActor(
            voiceType: VoiceType.Tenor,
            gender: Gender.Male);

        await _actorRepository.AddAsync(actor);

        var created = await _actorRepository.GetByIdAsync(actor.Id);
        created.Should().NotBeNull();
        created!.Name.Should().Be(actor.Name);
        created.VoiceType.Should().Be(VoiceType.Tenor);
        created.Gender.Should().Be(Gender.Male);

        created.Name = actor.Name + "123";
        created.VoiceType = VoiceType.Baritone;
        await _actorRepository.UpdateAsync(created);

        var updated = await _actorRepository.GetByIdAsync(actor.Id);
        updated!.Name.Should().Be(actor.Name);
        updated.VoiceType.Should().Be(VoiceType.Baritone);

        var allActors = await _actorRepository.GetAllAsync();

        allActors.Should().Contain(a => a.Id == actor.Id);

        var tenors = await _actorRepository.GetByVoiceTypeAsync(VoiceType.Baritone);

        tenors.Should().ContainSingle(a => a.Id == actor.Id);

        await _actorRepository.RemoveAsync(updated);

        var deleted = await _actorRepository.GetByIdAsync(actor.Id);
        deleted.Should().BeNull();
    }
}