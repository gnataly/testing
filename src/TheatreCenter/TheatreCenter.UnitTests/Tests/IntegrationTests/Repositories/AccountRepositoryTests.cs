using Microsoft.EntityFrameworkCore;
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
public class AccountRepositoryIt : IntegrationTestBase
{
    private readonly AccountFixture _accountFixture = new AccountFixture();
    private AccountRepository _repository;
    private Func<Task> _commitTransaction;
    private Func<Task> _rollbackTransaction;

    public AccountRepositoryIt(DatabaseFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    public override async Task InitializeAsync()
    {
        var (repository, commit, rollback) = await Fixture.CreateTransactionalRepositoryAsync<AccountRepository>();
        _repository = repository;
        _commitTransaction = commit;
        _rollbackTransaction = rollback;
    }

    public override async Task DisposeAsync()
    {
        await _rollbackTransaction();
    }

    [Fact]
    public async Task Account_FullCycle_WithUpgradeRequest()
    {
        // Arrange
        var account = _accountFixture.CreateAccount(
            accessLevel: AccessLevel.User,
            upgradeRequest: true,
            lastFavoritesViewDate: DateTime.UtcNow);

        await _repository.CreateAsync(account);

        // Assert 1 — проверить создание
        var created = await _repository.GetByUsernameAsync(account.Username);
        created.Should().NotBeNull();
        created!.Username.Should().Be(account.Username);
        created.AccessLevel.Should().Be(AccessLevel.User);
        created.UpgradeRequest.Should().BeTrue();

        // Act 4 — обработка запроса на апгрейд
        var processed = await _repository.ProcessUpgradeRequestAsync(account.Id, true);

        // Assert 4 — проверка обработки апгрейда
        processed.Should().BeTrue();
        var updatedAccount = await _repository.GetByIdAsync(account.Id);
        updatedAccount!.UpgradeRequest.Should().BeFalse();
        updatedAccount.AccessLevel.Should().Be(AccessLevel.Admin);

        // Act 6 — аутентификация
        var authenticated = await _repository.AuthenticateAsync(account.Username, account.PasswordHash);

        // Assert 6 — проверка аутентификации
        authenticated.Should().NotBeNull();
        authenticated!.Username.Should().Be(account.Username);

        // Act 7 — удалить аккаунт
        await _repository.DeleteAsync(account.Id);

        // Assert 7 — проверка удаления
        var deleted = await _repository.GetByIdAsync(account.Id);
        deleted.Should().BeNull();
    }
}