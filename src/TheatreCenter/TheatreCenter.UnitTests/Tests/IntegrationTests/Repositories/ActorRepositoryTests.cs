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

namespace TheatreCenter.Tests.IntegrationTests.Repositories;

[Collection("Database collection")]
[Trait("Category", TestCategories.Integration)]
public class ActorRepositoryIt(DatabaseFixture db) : IClassFixture<DatabaseFixture>
{
    private readonly ActorFixture _actorFixture = new ActorFixture();
    private ActorRepository CreateRepository()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;

        var context = new AppDbContext(options);
        var logger = NullLogger<ActorRepository>.Instance;
        return new ActorRepository(context, logger);
    }

    [Fact]
    public async Task Actor_FullCycle_WithVoiceType()
    {
        // Arrange
        var repo = CreateRepository();

        // Act 1 - создать актера
        var actor = _actorFixture.CreateActor(
            name: "Test Actor",
            voiceType: VoiceType.Tenor,
            gender: Gender.Male,
            birthDate: new DateTime(1985, 5, 15, 0, 0, 0, DateTimeKind.Utc));

        await repo.AddAsync(actor);

        // Assert 1 - проверить создание
        var created = await repo.GetByIdAsync(actor.Id);
        created.Should().NotBeNull();
        created!.Name.Should().Be("Test Actor");
        created.VoiceType.Should().Be(VoiceType.Tenor);
        created.Gender.Should().Be(Gender.Male);

        // Act 2 - обновить актера
        created.Name = "Updated Actor";
        created.VoiceType = VoiceType.Baritone;
        await repo.UpdateAsync(created);

        // Assert 2 - проверить обновление
        var updated = await repo.GetByIdAsync(actor.Id);
        updated!.Name.Should().Be("Updated Actor");
        updated.VoiceType.Should().Be(VoiceType.Baritone);

        // Act 3 - получить всех актеров
        var allActors = await repo.GetAllAsync();

        // Assert 3 - проверить получение всех
        allActors.Should().Contain(a => a.Id == actor.Id);

        // Act 4 - получить актеров по типу голоса
        var tenors = await repo.GetByVoiceTypeAsync(VoiceType.Baritone);

        // Assert 4 - проверить фильтрацию
        tenors.Should().ContainSingle(a => a.Id == actor.Id);

        // Act 5 - удалить актера
        await repo.RemoveAsync(updated);

        // Assert 5 - проверить удаление
        var deleted = await repo.GetByIdAsync(actor.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task Actor_GetByVoiceType_ReturnsCorrectActors()
    {
        // Arrange
        var repo = CreateRepository();

        var tenor1 = _actorFixture.CreateActor(voiceType: VoiceType.Tenor, name: "Tenor 1");
        var tenor2 = _actorFixture.CreateActor(voiceType: VoiceType.Tenor, name: "Tenor 2");
        var soprano = _actorFixture.CreateActor(voiceType: VoiceType.Soprano, name: "Soprano");

        await repo.AddAsync(tenor1);
        await repo.AddAsync(tenor2);
        await repo.AddAsync(soprano);

        // Act
        var tenors = await repo.GetByVoiceTypeAsync(VoiceType.Tenor);
        var sopranos = await repo.GetByVoiceTypeAsync(VoiceType.Soprano);

        // Assert
        tenors.Should().HaveCount(2);
        tenors.Should().Contain(a => a.Name == "Tenor 1");
        tenors.Should().Contain(a => a.Name == "Tenor 2");

        sopranos.Should().ContainSingle();
        sopranos.Should().Contain(a => a.Name == "Soprano");

        // Cleanup
        await repo.RemoveAsync(tenor1);
        await repo.RemoveAsync(tenor2);
        await repo.RemoveAsync(soprano);
    }
}