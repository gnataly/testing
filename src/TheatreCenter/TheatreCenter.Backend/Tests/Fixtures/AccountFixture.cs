using TheatreCenter.Domain.Models;
using TheatreCenter.Domain.Enums;
using AutoFixture;

namespace TheatreCenter.Tests.Fixtures;

public class AccountFixture
{
    private readonly IFixture _fixture;

    public AccountFixture()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        // Настраиваем автоматическую генерацию для строковых свойств
        _fixture.Customize<string>(c => c.FromFactory(() => Guid.NewGuid().ToString().Substring(0, 10)));
    }

    public Account CreateAccount(
        int? id = null,
        string? username = null,
        string? passwordHash = null,
        AccessLevel accessLevel = AccessLevel.User,
        bool upgradeRequest = false)
    {
        var account = _fixture.Build<Account>()
            .With(a => a.Id, id ?? _fixture.Create<int>())
            .With(a => a.Username, username ?? $"user{_fixture.Create<int>()}")
            .With(a => a.PasswordHash, passwordHash ?? $"hash{_fixture.Create<int>()}")
            .With(a => a.AccessLevel, accessLevel)
            .With(a => a.UpgradeRequest, upgradeRequest)
            .With(a => a.LastFavoritesViewDate, DateTime.UtcNow)
            .Create();

        return account;
    }
}