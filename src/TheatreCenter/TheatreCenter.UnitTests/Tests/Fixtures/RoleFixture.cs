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

        _fixture.Customize<string>(c => c.FromFactory(() =>
            Guid.NewGuid().ToString().Substring(0, 8)));
    }

    public Role CreateRole(
        int? id = null,
        string? name = null,
        RoleType roleType = RoleType.Main,
        int? musicalId = null)
    {
        var roleName = name ?? $"R{_fixture.Create<int>() % 1000}";
        if (roleName.Length > 50) roleName = roleName.Substring(0, 50);

        var role = _fixture.Build<Role>()
            .With(r => r.Id, id ?? _fixture.Create<int>() + 5)
            .With(r => r.Name, name ?? $"Role {_fixture.Create<int>()}")
            .With(r => r.RoleType, roleType)
            .With(r => r.MusicalId, musicalId ?? 1)
            .Without(r => r.Musical)
            .Without(r => r.ActorRoles)
            .Without(r => r.CastMembers)
            .Create();

        return role;
    }

    public Musical CreateMusical(
        int? id = null,
        string? title = null,
        AgeRestriction ageRestriction = AgeRestriction.EighteenPlus)
    {
        var musicalTitle = title ?? $"M{_fixture.Create<int>() % 1000}";
        if (musicalTitle.Length > 100) musicalTitle = musicalTitle.Substring(0, 100);

        var musical = _fixture.Build<Musical>()
            .With(m => m.Id, id ?? _fixture.Create<int>() + 5)
            .With(m => m.Title, title ?? $"Musical {_fixture.Create<int>()}")
            .With(m => m.Description, $"Description {_fixture.Create<int>()}")
            .With(m => m.Duration, TimeSpan.FromHours(2))
            .With(m => m.AgeRestriction, ageRestriction)
            .With(m => m.TheatreId, 1)
            .Without(m => m.Theatre)
            .Without(m => m.Shows)
            .Without(m => m.Roles)
            .Without(m => m.MusicalThemes)
            .Create();

        return musical;
    }
}