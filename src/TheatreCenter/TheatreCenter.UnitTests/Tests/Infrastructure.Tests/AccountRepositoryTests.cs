using Allure.Xunit.Attributes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TheatreCenter.Data;
using TheatreCenter.Data.Repositories;
using TheatreCenter.Domain.Enums;
using TheatreCenter.Domain.Models;
using TheatreCenter.Tests.Fixtures;
using TheatreCenter.UnitTests;
using Xunit;

namespace TheatreCenter.Tests.Repositories;

[AllureSuite("Account Repository Tests")]
[Trait("Category", TestCategories.Unit)]
public class AccountRepositoryTests : IClassFixture<AccountFixture>
{
    private readonly AccountFixture _fixture;

    public AccountRepositoryTests(AccountFixture fixture)
    {
        _fixture = fixture;
    }

    private AccountRepository GetInMemoryRepository()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);
        return new AccountRepository(context);
    }

    [Fact]
    [AllureFeature("GetByIdAsync")]
    [AllureStory("Positive case - account exists")]
    public async Task GetByIdAsync_AccountExists_ReturnsAccount()
    {
        
        var repository = GetInMemoryRepository();
        var account = _fixture.CreateAccount();

        await repository.CreateAsync(account);

        
        var result = await repository.GetByIdAsync(account.Id);

        
        result.Should().NotBeNull();
        result.Id.Should().Be(account.Id);
        result.Username.Should().Be(account.Username);
    }

    [Fact]
    [AllureFeature("GetByIdAsync")]
    [AllureStory("Negative case - account not found")]
    public async Task GetByIdAsync_AccountNotExists_ReturnsNull()
    {
        
        var repository = GetInMemoryRepository();

        
        var result = await repository.GetByIdAsync(999);

        
        result.Should().BeNull();
    }

    [Fact]
    [AllureFeature("GetByUsernameAsync")]
    [AllureStory("Positive case - username exists")]
    public async Task GetByUsernameAsync_UsernameExists_ReturnsAccount()
    {
        
        var repository = GetInMemoryRepository();
        var account = _fixture.CreateAccount(username: "testuser");

        await repository.CreateAsync(account);

        
        var result = await repository.GetByUsernameAsync("testuser");

        
        result.Should().NotBeNull();
        result.Username.Should().Be("testuser");
    }

    [Fact]
    [AllureFeature("GetByUsernameAsync")]
    [AllureStory("Negative case - username not found")]
    public async Task GetByUsernameAsync_UsernameNotExists_ReturnsNull()
    {
        
        var repository = GetInMemoryRepository();

        
        var result = await repository.GetByUsernameAsync("nonexistent");

        
        result.Should().BeNull();
    }

    [Fact]
    [AllureFeature("GetAllAsync")]
    [AllureStory("Positive case - returns all accounts")]
    public async Task GetAllAsync_AccountsExist_ReturnsAccounts()
    {
        
        var repository = GetInMemoryRepository();
        var account1 = _fixture.CreateAccount(username: "user1");
        var account2 = _fixture.CreateAccount(username: "user2");

        await repository.CreateAsync(account1);
        await repository.CreateAsync(account2);

        
        var result = await repository.GetAllAsync();

        
        result.Should().HaveCount(2);
        result.Should().Contain(a => a.Username == "user1");
        result.Should().Contain(a => a.Username == "user2");
    }

    [Fact]
    [AllureFeature("GetAllAsync")]
    [AllureStory("Negative case - no accounts")]
    public async Task GetAllAsync_NoAccounts_ReturnsEmpty()
    {
        
        var repository = GetInMemoryRepository();

        
        var result = await repository.GetAllAsync();

        
        result.Should().BeEmpty();
    }

    [Fact]
    [AllureFeature("CreateAsync")]
    [AllureStory("Positive case - creates account")]
    public async Task CreateAsync_ValidAccount_AddsToDatabase()
    {
        
        var repository = GetInMemoryRepository();
        var account = _fixture.CreateAccount();

        
        await repository.CreateAsync(account);

        
        var result = await repository.GetByIdAsync(account.Id);
        result.Should().NotBeNull();
    }

    [Fact]
    [AllureFeature("UpdateAsync")]
    [AllureStory("Positive case - updates account")]
    public async Task UpdateAsync_ValidAccount_UpdatesSuccessfully()
    {
        
        var repository = GetInMemoryRepository();
        var account = _fixture.CreateAccount(username: "original");

        await repository.CreateAsync(account);

        var existingAccount = await repository.GetByIdAsync(account.Id);
        existingAccount.Username = "updated";

        
        await repository.UpdateAsync(existingAccount);

        
        var result = await repository.GetByIdAsync(account.Id);
        result.Username.Should().Be("updated");
    }

    [Fact]
    [AllureFeature("DeleteAsync")]
    [AllureStory("Positive case - deletes account")]
    public async Task DeleteAsync_AccountExists_RemovesAccount()
    {
        
        var repository = GetInMemoryRepository();
        var account = _fixture.CreateAccount();

        await repository.CreateAsync(account);

        
        await repository.DeleteAsync(account.Id);

        
        var result = await repository.GetByIdAsync(account.Id);
        result.Should().BeNull();
    }

    [Fact]
    [AllureFeature("DeleteAsync")]
    [AllureStory("Negative case - account not found")]
    public async Task DeleteAsync_AccountNotExists_DoesNothing()
    {
        
        var repository = GetInMemoryRepository();

        
        await repository.Invoking(async r => await r.DeleteAsync(999))
            .Should().NotThrowAsync();
    }

    [Fact]
    [AllureFeature("AuthenticateAsync")]
    [AllureStory("Positive case - valid credentials")]
    public async Task AuthenticateAsync_ValidCredentials_ReturnsAccount()
    {
        
        var repository = GetInMemoryRepository();
        var account = _fixture.CreateAccount(username: "testuser", passwordHash: "correcthash");

        await repository.CreateAsync(account);

        
        var result = await repository.AuthenticateAsync("testuser", "correcthash");

        
        result.Should().NotBeNull();
        result.Username.Should().Be("testuser");
    }

    [Fact]
    [AllureFeature("AuthenticateAsync")]
    [AllureStory("Negative case - invalid credentials")]
    public async Task AuthenticateAsync_InvalidCredentials_ReturnsNull()
    {
        
        var repository = GetInMemoryRepository();
        var account = _fixture.CreateAccount(username: "testuser", passwordHash: "correcthash");

        await repository.CreateAsync(account);

        
        var result = await repository.AuthenticateAsync("testuser", "wronghash");

        
        result.Should().BeNull();
    }

    [Fact]
    [AllureFeature("GetFavoritesAsync")]
    [AllureStory("Positive case - returns favorites")]
    public async Task GetFavoritesAsync_AccountExists_ReturnsFavorites()
    {
        
        var repository = GetInMemoryRepository();
        var account = _fixture.CreateAccount();

        await repository.CreateAsync(account);
         

        
        var result = await repository.GetFavoritesAsync(account.Id);

        
        result.Should().NotBeNull();
    }

    [Fact]
    [AllureFeature("GetFavoritesAsync")]
    [AllureStory("Negative case - account not found")]
    public async Task GetFavoritesAsync_AccountNotExists_ReturnsNull()
    {
        
        var repository = GetInMemoryRepository();

        
        var result = await repository.GetFavoritesAsync(999);

        
        result.Should().BeNull();
    }

    [Fact]
    [AllureFeature("GetAccountsWithUpgradeRequestAsync")]
    [AllureStory("Positive case - returns accounts with requests")]
    public async Task GetAccountsWithUpgradeRequestAsync_AccountsExist_ReturnsAccounts()
    {
        
        var repository = GetInMemoryRepository();
        var account1 = _fixture.CreateAccount(upgradeRequest: true);
        var account2 = _fixture.CreateAccount(upgradeRequest: false);

        await repository.CreateAsync(account1);
        await repository.CreateAsync(account2);
         

        
        var result = await repository.GetAccountsWithUpgradeRequestAsync();

        
        result.Should().HaveCount(1);
        result.First().UpgradeRequest.Should().BeTrue();
    }

    [Fact]
    [AllureFeature("GetAccountsWithUpgradeRequestAsync")]
    [AllureStory("Negative case - no upgrade requests")]
    public async Task GetAccountsWithUpgradeRequestAsync_NoRequests_ReturnsEmpty()
    {
        
        var repository = GetInMemoryRepository();
        var account = _fixture.CreateAccount(upgradeRequest: false);

        await repository.CreateAsync(account);
         

        
        var result = await repository.GetAccountsWithUpgradeRequestAsync();

        
        result.Should().BeEmpty();
    }

    [Fact]
    [AllureFeature("ProcessUpgradeRequestAsync")]
    [AllureStory("Positive case - processes upgrade request")]
    public async Task ProcessUpgradeRequestAsync_ValidAccount_ReturnsTrue()
    {
        
        var repository = GetInMemoryRepository();
        var account = _fixture.CreateAccount(upgradeRequest: true, accessLevel: AccessLevel.User);

        await repository.CreateAsync(account);
         

        
        var result = await repository.ProcessUpgradeRequestAsync(account.Id, true);

        
        result.Should().BeTrue();
    }

    [Fact]
    [AllureFeature("ProcessUpgradeRequestAsync")]
    [AllureStory("Negative case - account not found")]
    public async Task ProcessUpgradeRequestAsync_AccountNotExists_ReturnsFalse()
    {
        
        var repository = GetInMemoryRepository();

        
        var result = await repository.ProcessUpgradeRequestAsync(999, true);

        
        result.Should().BeFalse();
    }
}