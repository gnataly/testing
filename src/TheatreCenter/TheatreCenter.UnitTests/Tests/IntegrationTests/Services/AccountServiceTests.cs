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
using Xunit.Abstractions;
using AutoFixture;
using TheatreCenter.UnitTests.Tests.IntegrationTests;

namespace TheatreCenter.Tests.IntegrationTests.Services;

[Collection("Database collection")]
[Trait("Category", TestCategories.Integration)]
public class AccountServiceIt : IntegrationTestBase
{
    private readonly AccountFixture _accountFixture = new AccountFixture();
    private AccountService _service;
    private Func<Task> _commitTransaction;
    private Func<Task> _rollbackTransaction;

    public AccountServiceIt(DatabaseFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    public override async Task InitializeAsync()
    {
        var context = await Fixture.CreateTransactionalContextAsync();
        var repository = new AccountRepository(context);
        _service = new AccountService(repository);

        _commitTransaction = async () => {
            await context.Database.CommitTransactionAsync();
            await context.DisposeAsync();
        };
        _rollbackTransaction = async () => {
            await context.Database.RollbackTransactionAsync();
            await context.DisposeAsync();
        };
    }

    public override async Task DisposeAsync()
    {
        await _rollbackTransaction();
    }

    [Fact]
    public async Task Account_FullCycle_WithFixtures()
    {
        // Используем фикстуру для генерации аккаунта
        var testAccount = _accountFixture.CreateAccount(
            username: "testuser",
            passwordHash: "testhash",
            accessLevel: AccessLevel.User
        );

        // Act 1 — регистрация аккаунта
        var registeredAccount = await _service.RegisterAsync(
            testAccount.Username,
            testAccount.PasswordHash,
            testAccount.AccessLevel
        );

        // Assert 1 — проверка регистрации
        registeredAccount.Should().NotBeNull();
        registeredAccount.Username.Should().Be(testAccount.Username);
        registeredAccount.AccessLevel.Should().Be(testAccount.AccessLevel);

        // Остальная часть теста...
        // Act 2 — аутентификация
        var authenticatedAccount = await _service.AuthenticateAsync(
            testAccount.Username,
            testAccount.PasswordHash
        );

        // Assert 2 — проверка аутентификации
        authenticatedAccount.Should().NotBeNull();
        authenticatedAccount.Id.Should().Be(registeredAccount.Id);

        // Остальные действия...
    }
}