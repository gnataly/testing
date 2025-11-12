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
        // Arrange
        var context = await Fixture.CreateTransactionalContextAsync();

        var actor = _actorFixture.CreateActor(name: "Test Actor");
        var role = _roleFixture.CreateRole(name: "Test Role");
        var show = _showFixture.CreateShow(date: DateTime.UtcNow.AddDays(7));

        context.Actors.Add(actor);
        context.Roles.Add(role);
        context.Shows.Add(show);
        await context.SaveChangesAsync();
        await context.Database.CommitTransactionAsync();
        await context.DisposeAsync();

        // Act 1 - создать участника каста
        var castMember = _castMemberFixture.CreateCastMember(
            showId: show.Id,
            roleId: role.Id,
            actorId: actor.Id,
            comment: "Main cast");

        await _repository.AddAsync(castMember);

        // Assert 1 - проверить создание
        var created = await _repository.GetByIdAsync(castMember.Id);
        created.Should().NotBeNull();
        created!.ShowId.Should().Be(show.Id);
        created.RoleId.Should().Be(role.Id);
        created.ActorId.Should().Be(actor.Id);
        created.Comment.Should().Be("Main cast");

        // Act 2 - обновить участника каста
        created.Comment = "Updated comment";
        await _repository.UpdateAsync(created);

        // Assert 2 - проверить обновление
        var updated = await _repository.GetByIdAsync(castMember.Id);
        updated!.Comment.Should().Be("Updated comment");

        // Act 3 - получить участников каста по показу
        var showCast = await _repository.GetByShowIdAsync(show.Id);

        // Assert 3 - проверить фильтрацию по показу
        showCast.Should().ContainSingle(cm => cm.Id == castMember.Id);

        // Act 4 - получить участников каста по актеру
        var actorCast = await _repository.GetByActorIdAsync(actor.Id);

        // Assert 4 - проверить фильтрацию по актеру
        actorCast.Should().ContainSingle(cm => cm.Id == castMember.Id);

        // Act 5 - получить участника каста по показу и роли
        var specificCastMember = await _repository.GetByShowAndRoleAsync(show.Id, role.Id);

        // Assert 5 - проверить получение по показу и роли
        specificCastMember.Should().NotBeNull();
        specificCastMember!.Id.Should().Be(castMember.Id);

        // Act 6 - удалить участника каста
        await _repository.RemoveAsync(updated);

        // Assert 6 - проверить удаление
        var deleted = await _repository.GetByIdAsync(castMember.Id);
        deleted.Should().BeNull();
    }
}