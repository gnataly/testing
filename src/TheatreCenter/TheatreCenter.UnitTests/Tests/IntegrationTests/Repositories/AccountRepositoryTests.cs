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
public class AccountRepositoryIt(DatabaseFixture db)
    : IClassFixture<DatabaseFixture>
{
    private readonly AccountFixture _accountFixture = new AccountFixture();
    private readonly ActorFixture _actorFixture = new ActorFixture();
    private AccountRepository CreateRepository()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;

        var context = new AppDbContext(options);
        return new AccountRepository(context);
    }

    [Fact]
    public async Task Account_FullCycle_WithFavoritesAndUpgradeRequest()
    {
        // Arrange
        var repo = CreateRepository();

        // Act 1 — создать аккаунт
        var account = _accountFixture.CreateAccount(
            username: "testuser",
            passwordHash: "hashed_password",
            accessLevel: AccessLevel.User,
            upgradeRequest: true,
            lastFavoritesViewDate: DateTime.UtcNow);

        await repo.CreateAsync(account);

        // Assert 1 — проверить создание
        var created = await repo.GetByUsernameAsync("testuser");
        created.Should().NotBeNull();
        created!.Username.Should().Be("testuser");
        created.AccessLevel.Should().Be(AccessLevel.User);
        created.UpgradeRequest.Should().BeTrue();

        // Act 2 — добавить в избранное
        var actor = _actorFixture.CreateActor(id: 100, name: "Test Actor");
        var theatre = new Theatre(200, "Test Theatre", "Test info");
        var musical = new Musical(300, "Test Musical", "Description",
            TimeSpan.FromHours(2), AgeRestriction.TwelvePlus, 200);

        // Добавляем связанные сущности в контекст
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;
        using var context = new AppDbContext(options);
        context.Actors.Add(actor);
        context.Theatres.Add(theatre);
        context.Musicals.Add(musical);
        await context.SaveChangesAsync();

        // Добавляем в избранное
        var actorAdded = await repo.AddFavoriteActorAsync(account.Id, actor.Id);
        var theatreAdded = await repo.AddFavoriteTheatreAsync(account.Id, theatre.Id);
        var musicalAdded = await repo.AddFavoriteMusicalAsync(account.Id, musical.Id);

        // Assert 2 — проверка добавления в избранное
        actorAdded.Should().BeTrue();
        theatreAdded.Should().BeTrue();
        musicalAdded.Should().BeTrue();

        // Act 3 — получить избранное
        var favorites = await repo.GetFavoritesAsync(account.Id);

        // Assert 3 — проверка избранного
        favorites.Should().NotBeNull();
        favorites!.Actors.Should().ContainSingle(a => a.Id == actor.Id);
        favorites.Theatres.Should().ContainSingle(t => t.Id == theatre.Id);
        favorites.Musicals.Should().ContainSingle(m => m.Id == musical.Id);

        // Act 4 — обработка запроса на апгрейд
        var processed = await repo.ProcessUpgradeRequestAsync(account.Id, true);

        // Assert 4 — проверка обработки апгрейда
        processed.Should().BeTrue();
        var updatedAccount = await repo.GetByIdAsync(account.Id);
        updatedAccount!.UpgradeRequest.Should().BeFalse();
        updatedAccount.AccessLevel.Should().Be(AccessLevel.Admin);

        // Act 5 — удалить из избранного
        var actorRemoved = await repo.RemoveFavoriteActorAsync(account.Id, actor.Id);
        var theatreRemoved = await repo.RemoveFavoriteTheatreAsync(account.Id, theatre.Id);
        var musicalRemoved = await repo.RemoveFavoriteMusicalAsync(account.Id, musical.Id);

        // Assert 5 — проверка удаления из избранного
        actorRemoved.Should().BeTrue();
        theatreRemoved.Should().BeTrue();
        musicalRemoved.Should().BeTrue();

        // Act 6 — аутентификация
        var authenticated = await repo.AuthenticateAsync("testuser", "hashed_password");

        // Assert 6 — проверка аутентификации
        authenticated.Should().NotBeNull();
        authenticated!.Username.Should().Be("testuser");

        // Act 7 — удалить аккаунт
        await repo.DeleteAsync(account.Id);

        // Assert 7 — проверка удаления
        var deleted = await repo.GetByIdAsync(account.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task Account_GetAccountsWithUpgradeRequest_ReturnsCorrectAccounts()
    {
        // Arrange
        var repo = CreateRepository();

        var account1 = _accountFixture.CreateAccount(username: "user1", upgradeRequest: true);
        var account2 = _accountFixture.CreateAccount(username: "user2", upgradeRequest: false);
        var account3 = _accountFixture.CreateAccount(username: "user3", upgradeRequest: true);

        await repo.CreateAsync(account1);
        await repo.CreateAsync(account2);
        await repo.CreateAsync(account3);

        // Act
        var upgradeRequests = await repo.GetAccountsWithUpgradeRequestAsync();

        // Assert
        upgradeRequests.Should().HaveCount(2);
        upgradeRequests.Should().Contain(a => a.Username == "user1");
        upgradeRequests.Should().Contain(a => a.Username == "user3");
        upgradeRequests.Should().NotContain(a => a.Username == "user2");

        // Cleanup
        await repo.DeleteAsync(account1.Id);
        await repo.DeleteAsync(account2.Id);
        await repo.DeleteAsync(account3.Id);
    }
}