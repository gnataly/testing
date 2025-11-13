using Allure.Xunit.Attributes;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TheatreCenter.Domain.Enums;
using TheatreCenter.Domain.Interfaces.Repositories;
using TheatreCenter.Domain.Models;
using TheatreCenter.Services.Services;
using TheatreCenter.Tests.Fixtures;
using TheatreCenter.UnitTests;
using Xunit;

namespace TheatreCenter.Tests.Services;

[AllureSuite("Account Service Tests")]
[AllureSubSuite("London Style (with Mocks)")]
[Trait("Category", TestCategories.Unit)]
public class AccountServiceMockTests : IClassFixture<AccountFixture>
{
    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly Mock<ILogger<AccountService>> _loggerMock;
    private readonly AccountService _sut;
    private readonly AccountFixture _fixture;

    public AccountServiceMockTests(AccountFixture fixture)
    {
        _fixture = fixture;
        _accountRepositoryMock = new Mock<IAccountRepository>();
        _loggerMock = new Mock<ILogger<AccountService>>();
        _sut = new AccountService(_accountRepositoryMock.Object);
    }

    [Fact]
    [AllureFeature("GetByIdAsync")]
    [AllureStory("Positive case - account exists")]
    public async Task GetByIdAsync_AccountExists_ReturnsAccount()
    {
        
        var accountId = 1;
        var expectedAccount = _fixture.CreateAccount(id: accountId);

        _accountRepositoryMock
            .Setup(repo => repo.GetByIdAsync(accountId))
            .ReturnsAsync(expectedAccount);

        
        var result = await _sut.GetByIdAsync(accountId);

        
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedAccount);
        _accountRepositoryMock.Verify(repo => repo.GetByIdAsync(accountId), Times.Once);
    }

    [Fact]
    [AllureFeature("GetByIdAsync")]
    [AllureStory("Negative case - account not found")]
    public async Task GetByIdAsync_AccountNotExists_ReturnsNull()
    {
        
        var accountId = 1;

        _accountRepositoryMock
            .Setup(repo => repo.GetByIdAsync(accountId))
            .ReturnsAsync((Account?)null);

        
        var result = await _sut.GetByIdAsync(accountId);

        
        result.Should().BeNull();
        _accountRepositoryMock.Verify(repo => repo.GetByIdAsync(accountId), Times.Once);
    }

    [Fact]
    [AllureFeature("AuthenticateAsync")]
    [AllureStory("Positive case - valid credentials")]
    public async Task AuthenticateAsync_ValidCredentials_ReturnsAccount()
    {
        
        var username = "testuser";
        var passwordHash = "hashedpassword";
        var expectedAccount = _fixture.CreateAccount(username: username, passwordHash: passwordHash);

        _accountRepositoryMock
            .Setup(repo => repo.AuthenticateAsync(username, passwordHash))
            .ReturnsAsync(expectedAccount);

        
        var result = await _sut.AuthenticateAsync(username, passwordHash);

        
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedAccount);
        _accountRepositoryMock.Verify(repo => repo.AuthenticateAsync(username, passwordHash), Times.Once);
    }

    [Fact]
    [AllureFeature("AuthenticateAsync")]
    [AllureStory("Negative case - invalid password")]
    public async Task AuthenticateAsync_InvalidPassword_ThrowsUnauthorizedAccessException()
    {
        
        var username = "testuser";
        var passwordHash = "hashedpassword";
        var wrongPasswordHash = "wrongpassword";
        var account = _fixture.CreateAccount(username: username, passwordHash: passwordHash);

        _accountRepositoryMock
            .Setup(repo => repo.AuthenticateAsync(username, wrongPasswordHash))
            .ReturnsAsync(account);

        
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _sut.AuthenticateAsync(username, wrongPasswordHash));
    }

    [Fact]
    [AllureFeature("AuthenticateAsync")]
    [AllureStory("Negative case - account not found")]
    public async Task AuthenticateAsync_AccountNotFound_ThrowsUnauthorizedAccessException()
    {
        
        var username = "testuser";
        var passwordHash = "hashedpassword";

        _accountRepositoryMock
            .Setup(repo => repo.AuthenticateAsync(username, passwordHash))
            .ReturnsAsync((Account?)null);

        
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _sut.AuthenticateAsync(username, passwordHash));
    }

    [Fact]
    [AllureFeature("RegisterAsync")]
    [AllureStory("Positive case - new user")]
    public async Task RegisterAsync_NewUser_ReturnsAccount()
    {
        
        var username = "newuser";
        var passwordHash = "hashedpassword";

        _accountRepositoryMock
            .Setup(repo => repo.GetByUsernameAsync(username))
            .ReturnsAsync((Account?)null);
        _accountRepositoryMock
            .Setup(repo => repo.CreateAsync(It.IsAny<Account>()))
            .Returns(Task.CompletedTask);

        
        var result = await _sut.RegisterAsync(username, passwordHash);

        
        result.Should().NotBeNull();
        result.Username.Should().Be(username);
        result.PasswordHash.Should().Be(passwordHash);
        _accountRepositoryMock.Verify(repo => repo.GetByUsernameAsync(username), Times.Once);
        _accountRepositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<Account>()), Times.Once);
    }

    [Fact]
    [AllureFeature("RegisterAsync")]
    [AllureStory("Negative case - username exists")]
    public async Task RegisterAsync_UsernameExists_ThrowsArgumentException()
    {
        
        var username = "existinguser";
        var passwordHash = "hashedpassword";
        var existingAccount = _fixture.CreateAccount(username: username);

        _accountRepositoryMock
            .Setup(repo => repo.GetByUsernameAsync(username))
            .ReturnsAsync(existingAccount);

        
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.RegisterAsync(username, passwordHash));
        _accountRepositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<Account>()), Times.Never);
    }

    [Fact]
    [AllureFeature("UpdateAsync")]
    [AllureStory("Positive case - account updated")]
    public async Task UpdateAsync_ValidAccount_UpdatesSuccessfully()
    {
        
        var account = _fixture.CreateAccount(id: 1);

        _accountRepositoryMock
            .Setup(repo => repo.GetByIdAsync(account.Id))
            .ReturnsAsync(account);
        _accountRepositoryMock
            .Setup(repo => repo.UpdateAsync(account))
            .Returns(Task.CompletedTask);

        
        await _sut.UpdateAsync(account);

        
        _accountRepositoryMock.Verify(repo => repo.GetByIdAsync(account.Id), Times.Once);
        _accountRepositoryMock.Verify(repo => repo.UpdateAsync(account), Times.Once);
    }

    [Fact]
    [AllureFeature("UpdateAsync")]
    [AllureStory("Negative case - account not found")]
    public async Task UpdateAsync_AccountNotFound_ThrowsKeyNotFoundException()
    {
        
        var account = _fixture.CreateAccount(id: 1);

        _accountRepositoryMock
            .Setup(repo => repo.GetByIdAsync(account.Id))
            .ReturnsAsync((Account?)null);

        
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.UpdateAsync(account));
        _accountRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Account>()), Times.Never);
    }

    [Fact]
    [AllureFeature("DeleteAsync")]
    [AllureStory("Positive case - account deleted")]
    public async Task DeleteAsync_ValidId_DeletesSuccessfully()
    {
        
        var accountId = 1;

        _accountRepositoryMock
            .Setup(repo => repo.DeleteAsync(accountId))
            .Returns(Task.CompletedTask);

        
        await _sut.DeleteAsync(accountId);

        
        _accountRepositoryMock.Verify(repo => repo.DeleteAsync(accountId), Times.Once);
    }

    [Fact]
    [AllureFeature("ChangePasswordAsync")]
    [AllureStory("Positive case - password changed")]
    public async Task ChangePasswordAsync_ValidAccount_ChangesPassword()
    {
        
        var accountId = 1;
        var newPasswordHash = "newhashedpassword";
        var account = _fixture.CreateAccount(id: accountId);

        _accountRepositoryMock
            .Setup(repo => repo.GetByIdAsync(accountId))
            .ReturnsAsync(account);
        _accountRepositoryMock
            .Setup(repo => repo.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        
        await _sut.ChangePasswordAsync(accountId, newPasswordHash);

        
        account.PasswordHash.Should().Be(newPasswordHash);
        _accountRepositoryMock.Verify(repo => repo.GetByIdAsync(accountId), Times.Once);
        _accountRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    [AllureFeature("ChangePasswordAsync")]
    [AllureStory("Negative case - account not found")]
    public async Task ChangePasswordAsync_AccountNotFound_ThrowsArgumentException()
    {
        
        var accountId = 1;
        var newPasswordHash = "newhashedpassword";

        _accountRepositoryMock
            .Setup(repo => repo.GetByIdAsync(accountId))
            .ReturnsAsync((Account?)null);

        
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.ChangePasswordAsync(accountId, newPasswordHash));
        _accountRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    [AllureFeature("SubmitUpgradeRequestAsync")]
    [AllureStory("Positive case - upgrade request submitted")]
    public async Task SubmitUpgradeRequestAsync_ValidAccount_ReturnsTrue()
    {
        
        var accountId = 1;
        var account = _fixture.CreateAccount(id: accountId);

        _accountRepositoryMock
            .Setup(repo => repo.GetByIdAsync(accountId))
            .ReturnsAsync(account);
        _accountRepositoryMock
            .Setup(repo => repo.UpdateAsync(account))
            .Returns(Task.CompletedTask);

        
        var result = await _sut.SubmitUpgradeRequestAsync(accountId);

        
        result.Should().BeTrue();
        account.UpgradeRequest.Should().BeTrue();
        _accountRepositoryMock.Verify(repo => repo.GetByIdAsync(accountId), Times.Once);
        _accountRepositoryMock.Verify(repo => repo.UpdateAsync(account), Times.Once);
    }

    [Fact]
    [AllureFeature("SubmitUpgradeRequestAsync")]
    [AllureStory("Negative case - account not found")]
    public async Task SubmitUpgradeRequestAsync_AccountNotFound_ReturnsFalse()
    {
        
        var accountId = 1;

        _accountRepositoryMock
            .Setup(repo => repo.GetByIdAsync(accountId))
            .ReturnsAsync((Account?)null);

        
        var result = await _sut.SubmitUpgradeRequestAsync(accountId);

        
        result.Should().BeFalse();
        _accountRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Account>()), Times.Never);
    }
}