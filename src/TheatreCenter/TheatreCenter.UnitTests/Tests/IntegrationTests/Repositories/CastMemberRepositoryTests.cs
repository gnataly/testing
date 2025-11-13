using Microsoft.EntityFrameworkCore;
using TheatreCenter.Data;
using TheatreCenter.Data.Repositories;
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
public class CastMemberRepositoryIt : IntegrationTestBase
{
    private readonly CastMemberFixture _castMemberFixture = new CastMemberFixture();
    private readonly ShowFixture _showFixture = new ShowFixture();
    private readonly RoleFixture _roleFixture = new RoleFixture();
    private readonly ActorFixture _actorFixture = new ActorFixture();
    private readonly MusicalFixture _musicalFixture = new MusicalFixture();
    private readonly TheatreFixture _theatreFixture = new TheatreFixture();
    private CastMemberRepository _repository;
    private Func<Task> _commitTransaction;
    private Func<Task> _rollbackTransaction;

    public CastMemberRepositoryIt(DatabaseFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    public override async Task InitializeAsync()
    {
        var (repository, commit, rollback) = await Fixture.CreateTransactionalRepositoryAsync<CastMemberRepository>();
        _repository = repository;
        _commitTransaction = commit;
        _rollbackTransaction = rollback;
    }

    public override async Task DisposeAsync()
    {
        await _rollbackTransaction();
    }

    [Fact]
    public async Task CastMember_FullCycle_WithRelationships()
    {

        //// Arrange - Создаем все необходимые сущности с нуля
        //var context = await Fixture.CreateTransactionalContextAsync();

        //// Создаем театр
        //var theatre = _theatreFixture.CreateTheatre();
        //context.Theatres.Add(theatre);
        //await context.SaveChangesAsync();

        //// Создаем мюзикл
        //var musical = _musicalFixture.CreateMusical(
        //    theatreId: theatre.Id);
        //context.Musicals.Add(musical);
        //await context.SaveChangesAsync();

        //// Создаем актера
        //var actor = _actorFixture.CreateActor();
        //context.Actors.Add(actor);
        //await context.SaveChangesAsync();

        //// Создаем роль
        //var role = _roleFixture.CreateRole(
        //    musicalId: musical.Id);
        //context.Roles.Add(role);
        //await context.SaveChangesAsync();

        //// Создаем показ
        //var show = _showFixture.CreateShow(
        //    musicalId: musical.Id,
        //    date: DateTime.UtcNow.AddDays(7));
        //context.Shows.Add(show);
        //await context.SaveChangesAsync();

        //await context.Database.CommitTransactionAsync();
        

        // Act 1 - создать участника каста
        var castMember = _castMemberFixture.CreateCastMember();

        await _repository.AddAsync(castMember);

        // Assert 1 - проверить создание
        var created = await _repository.GetByIdAsync(castMember.Id);
        created.Should().NotBeNull();
        created!.ShowId.Should().Be(castMember.ShowId);
        created.RoleId.Should().Be(castMember.RoleId);
        created.ActorId.Should().Be(castMember.ActorId);
        created.Comment.Should().Be(castMember.Comment);

        // Act 2 - обновить участника каста
        created.Comment = castMember.Comment + "123";
        await _repository.UpdateAsync(created);

        // Assert 2 - проверить обновление
        var updated = await _repository.GetByIdAsync(castMember.Id);
        updated!.Comment.Should().Be(castMember.Comment);

        // Act 3 - получить участников каста по показу
        var showCast = await _repository.GetByShowIdAsync(castMember.ShowId);

        // Assert 3 - проверить фильтрацию по показу
        showCast.Should().ContainSingle(cm => cm.Id == castMember.Id);

        // Act 4 - получить участников каста по актеру
        var actorCast = await _repository.GetByActorIdAsync(castMember.ActorId);

        // Assert 4 - проверить фильтрацию по актеру
        actorCast.Should().ContainSingle(cm => cm.Id == castMember.Id);

        // Act 6 - удалить участника каста
        await _repository.RemoveAsync(updated);

        // Assert 6 - проверить удаление
        var deleted = await _repository.GetByIdAsync(castMember.Id);
        deleted.Should().BeNull();

    }
}