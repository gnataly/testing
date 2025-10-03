using TheatreCenter.Domain.Models;
using TheatreCenter.Domain.Enums;
using AutoFixture;

namespace TheatreCenter.Tests.Fixtures;

public class RoleFixture
{
    private readonly IFixture _fixture;

    public RoleFixture()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _fixture.Customize<string>(c => c.FromFactory(() => Guid.NewGuid().ToString().Substring(0, 10)));
    }

    public Role CreateRole(
        int? id = null,
        string? name = null,
        RoleType roleType = RoleType.Main,
        int? musicalId = null)
    {
        var role = _fixture.Build<Role>()
            .With(r => r.Id, id ?? _fixture.Create<int>())
            .With(r => r.Name, name ?? $"Role {_fixture.Create<int>()}")
            .With(r => r.RoleType, roleType)
            .With(r => r.MusicalId, musicalId ?? _fixture.Create<int>())
            .Without(r => r.ActorRoles)
            .Create();

        return role;
    }

    public Musical CreateMusical(int? id = null, string? title = null)
    {
        var musical = _fixture.Build<Musical>()
            .With(m => m.Id, id ?? _fixture.Create<int>())
            .With(m => m.Title, title ?? $"Musical {_fixture.Create<int>()}")
            .With(m => m.Duration, TimeSpan.FromHours(2))
            .Without(m => m.Shows)
            .Without(m => m.Roles)
            .Create();

        return musical;
    }
}