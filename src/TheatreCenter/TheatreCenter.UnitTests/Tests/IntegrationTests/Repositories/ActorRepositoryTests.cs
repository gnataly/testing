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

[Collection("Database collection")]
[Trait("Category", TestCategories.Integration)]
public class ActorRepositoryIt : IntegrationTestBase
{
    private readonly ActorFixture _actorFixture = new ActorFixture();
    private ActorRepository _repository;
    private Func<Task> _commitTransaction;
    private Func<Task> _rollbackTransaction;

    public ActorRepositoryIt(DatabaseFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    public override async Task InitializeAsync()
    {
        var (repository, commit, rollback) = await Fixture.CreateTransactionalRepositoryAsync<ActorRepository>();
        _repository = repository;
        _commitTransaction = commit;
        _rollbackTransaction = rollback;
    }

    public override async Task DisposeAsync()
    {
        await _rollbackTransaction();
    }

    [Fact]
    public async Task Actor_FullCycle_WithVoiceType()
    {
        // Act 1 - создать актера
        var actor = _actorFixture.CreateActor(
            voiceType: VoiceType.Tenor,
            gender: Gender.Male);

        await _repository.AddAsync(actor);

        // Assert 1 - проверить создание
        var created = await _repository.GetByIdAsync(actor.Id);
        created.Should().NotBeNull();
        created!.Name.Should().Be(actor.Name);
        created.VoiceType.Should().Be(VoiceType.Tenor);
        created.Gender.Should().Be(Gender.Male);

        // Act 2 - обновить актера
        created.Name = actor.Name + "123";
        created.VoiceType = VoiceType.Baritone;
        await _repository.UpdateAsync(created);

        // Assert 2 - проверить обновление
        var updated = await _repository.GetByIdAsync(actor.Id);
        updated!.Name.Should().Be(actor.Name);
        updated.VoiceType.Should().Be(VoiceType.Baritone);

        // Act 3 - получить всех актеров
        var allActors = await _repository.GetAllAsync();

        // Assert 3 - проверить получение всех
        allActors.Should().Contain(a => a.Id == actor.Id);

        // Act 4 - получить актеров по типу голоса
        var tenors = await _repository.GetByVoiceTypeAsync(VoiceType.Baritone);

        // Assert 4 - проверить фильтрацию
        tenors.Should().ContainSingle(a => a.Id == actor.Id);

        // Act 5 - удалить актера
        await _repository.RemoveAsync(updated);

        // Assert 5 - проверить удаление
        var deleted = await _repository.GetByIdAsync(actor.Id);
        deleted.Should().BeNull();
    }
}