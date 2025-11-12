using TheatreCenter.Data;
using TheatreCenter.Domain.Models;
using TheatreCenter.Domain.Enums;
using TheatreCenter.Services.Services;
using TheatreCenter.Tests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TheatreCenter.Data.Repositories;
using TheatreCenter.UnitTests.Tests.Database;
using TheatreCenter.UnitTests;
using Xunit;

namespace TheatreCenter.Tests.IntegrationTests.Services;

[Collection("Database collection")]
[Trait("Category", TestCategories.Integration)]
public class AccountServiceIt(DatabaseFixture db) : IClassFixture<DatabaseFixture>
{
    private readonly AccountFixture _accountFixture = new AccountFixture();

    private AccountService CreateService()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;

        var context = new AppDbContext(options);
        var repository = new AccountRepository(context);
        return new AccountService(repository);
    }

    [Fact]
    public async Task Account_FullCycle_WithFixtures()
    {
        var service = CreateService();

        // Используем фикстуру для генерации аккаунта
        var testAccount = _accountFixture.CreateAccount(
            username: "testuser",
            passwordHash: "testhash",
            accessLevel: AccessLevel.User
        );

        // Act 1 — регистрация аккаунта
        var registeredAccount = await service.RegisterAsync(
            testAccount.Username,
            testAccount.PasswordHash,
            testAccount.AccessLevel
        );

        // Assert 1 — проверка регистрации
        registeredAccount.Should().NotBeNull();
        registeredAccount.Username.Should().Be(testAccount.Username);
        registeredAccount.AccessLevel.Should().Be(testAccount.AccessLevel);

        // Act 2 — аутентификация
        var authenticatedAccount = await service.AuthenticateAsync(
            testAccount.Username,
            testAccount.PasswordHash
        );

        // Assert 2 — проверка аутентификации
        authenticatedAccount.Should().NotBeNull();
        authenticatedAccount.Id.Should().Be(registeredAccount.Id);

        // Act 3 — обновление пароля
        var newPasswordHash = "newtesthash";
        await service.ChangePasswordAsync(registeredAccount.Id, newPasswordHash);

        // Act 4 — запрос апгрейда
        var upgradeResult = await service.SubmitUpgradeRequestAsync(registeredAccount.Id);
        upgradeResult.Should().BeTrue();

        // Act 5 — получение аккаунта по ID
        var retrievedAccount = await service.GetByIdAsync(registeredAccount.Id);
        retrievedAccount.Should().NotBeNull();
        retrievedAccount.UpgradeRequest.Should().BeTrue();

        // Act 6 — удаление аккаунта
        await service.DeleteAsync(registeredAccount.Id);

        // Assert 6 — проверка удаления
        var deletedAccount = await service.GetByIdAsync(registeredAccount.Id);
        deletedAccount.Should().BeNull();
    }
}