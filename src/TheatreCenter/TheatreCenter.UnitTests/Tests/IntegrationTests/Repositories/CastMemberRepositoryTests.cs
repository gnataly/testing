using Microsoft.EntityFrameworkCore;
using TheatreCenter.Data;
using TheatreCenter.Data.Repositories;
using TheatreCenter.Domain.Models;
using TheatreCenter.Tests.Fixtures;
using FluentAssertions;
using TheatreCenter.UnitTests.Tests.Database;
using TheatreCenter.UnitTests;
using Xunit;

namespace TheatreCenter.Tests.IntegrationTests.Repositories;

[Collection("Database collection")]
[Trait("Category", TestCategories.Integration)]
public class CastMemberRepositoryIt(DatabaseFixture db) : IClassFixture<DatabaseFixture>
{
    private readonly CastMemberFixture _castMemberFixture = new CastMemberFixture();
    private readonly ShowFixture _showFixture = new ShowFixture();
    private readonly RoleFixture _roleFixture = new RoleFixture();
    private readonly ActorFixture _actorFixture = new ActorFixture();
    private CastMemberRepository CreateRepository()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;

        var context = new AppDbContext(options);
        return new CastMemberRepository(context);
    }

    [Fact]
    public async Task CastMember_FullCycle_WithRelationships()
    {
        // Arrange
        var repo = CreateRepository();

        // Создаем необходимые сущности
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;
        using var context = new AppDbContext(options);

        var actor = _actorFixture.CreateActor(name: "Test Actor");
        //var musical = castMemberFixture.CreateMusical(title: "Test Musical");
        var role = _roleFixture.CreateRole(name: "Test Role");
        var show = _showFixture.CreateShow(date: DateTime.UtcNow.AddDays(7));

        context.Actors.Add(actor);
        //context.Musicals.Add(musical);
        context.Roles.Add(role);
        context.Shows.Add(show);
        await context.SaveChangesAsync();

        // Act 1 - создать участника каста
        var castMember = _castMemberFixture.CreateCastMember(
            showId: show.Id,
            roleId: role.Id,
            actorId: actor.Id,
            comment: "Main cast");

        await repo.AddAsync(castMember);

        // Assert 1 - проверить создание
        var created = await repo.GetByIdAsync(castMember.Id);
        created.Should().NotBeNull();
        created!.ShowId.Should().Be(show.Id);
        created.RoleId.Should().Be(role.Id);
        created.ActorId.Should().Be(actor.Id);
        created.Comment.Should().Be("Main cast");

        // Act 2 - обновить участника каста
        created.Comment = "Updated comment";
        await repo.UpdateAsync(created);

        // Assert 2 - проверить обновление
        var updated = await repo.GetByIdAsync(castMember.Id);
        updated!.Comment.Should().Be("Updated comment");

        // Act 3 - получить участников каста по показу
        var showCast = await repo.GetByShowIdAsync(show.Id);

        // Assert 3 - проверить фильтрацию по показу
        showCast.Should().ContainSingle(cm => cm.Id == castMember.Id);

        // Act 4 - получить участников каста по актеру
        var actorCast = await repo.GetByActorIdAsync(actor.Id);

        // Assert 4 - проверить фильтрацию по актеру
        actorCast.Should().ContainSingle(cm => cm.Id == castMember.Id);

        // Act 5 - получить участника каста по показу и роли
        var specificCastMember = await repo.GetByShowAndRoleAsync(show.Id, role.Id);

        // Assert 5 - проверить получение по показу и роли
        specificCastMember.Should().NotBeNull();
        specificCastMember!.Id.Should().Be(castMember.Id);

        // Act 6 - удалить участника каста
        await repo.RemoveAsync(updated);

        // Assert 6 - проверить удаление
        var deleted = await repo.GetByIdAsync(castMember.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task CastMember_GetByShowAndRole_ReturnsCorrectCastMember()
    {
        // Arrange
        var repo = CreateRepository();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;
        using var context = new AppDbContext(options);
        //using (StreamWriter writer = new StreamWriter(@"C:\Users\gnata\OneDrive\Документы\log.txt"))
        //{
            var actor = _actorFixture.CreateActor();
            //var musical = castMemberFixture.CreateMusical();
            //writer.WriteLine($"musicalid = {musical.Id}");
            var role1 = _roleFixture.CreateRole(name: "Role 1");
            var role2 = _roleFixture.CreateRole(name: "Role 2");
            var show = _showFixture.CreateShow();

            
            context.Roles.Add(role1);
            context.Roles.Add(role2);
            context.Shows.Add(show);
            await context.SaveChangesAsync();

            //writer.WriteLine($"actid = {actor.Id}");
            var castMember = _castMemberFixture.CreateCastMember(
                showId: show.Id,
                roleId: role1.Id,
                actorId: actor.Id);

            await repo.AddAsync(castMember);

            // Act
            var foundCastMember = await repo.GetByShowAndRoleAsync(show.Id, role1.Id);
            var notFoundCastMember = await repo.GetByShowAndRoleAsync(show.Id, role2.Id);

            // Assert
            foundCastMember.Should().NotBeNull();
            foundCastMember!.Id.Should().Be(castMember.Id);
            notFoundCastMember.Should().BeNull();

            // Cleanup
            await repo.RemoveAsync(castMember);
        //}
    }
}