using TheatreCenter.Domain.Models;
using AutoFixture;
using TheatreCenter.Domain.Enums;

namespace TheatreCenter.Tests.Fixtures;

public class CastMemberFixture
{
    private readonly IFixture _fixture;

    public CastMemberFixture()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _fixture.Customize<string>(c => c.FromFactory(() => Guid.NewGuid().ToString().Substring(0, 10)));
    }

    public CastMember CreateCastMember(
        int? id = null,
        int? showId = null,
        int? roleId = null,
        int? actorId = null,
        string? comment = null)
    {
        var castMember = _fixture.Build<CastMember>()
            .With(cm => cm.Id, id ?? _fixture.Create<int>())
            .With(cm => cm.ShowId, showId ?? _fixture.Create<int>())
            .With(cm => cm.RoleId, roleId ?? _fixture.Create<int>())
            .With(cm => cm.ActorId, actorId ?? _fixture.Create<int>())
            .With(cm => cm.Comment, comment ?? $"Comment {_fixture.Create<int>()}")
            .Without(cm => cm.Show)
            .Without(cm => cm.Role)
            .Without(cm => cm.Actor)
            .Create();

        return castMember;
    }

    public Show CreateShow(int? id = null, int? musicalId = null, DateTime? date = null)
    {
        var show = _fixture.Build<Show>()
            .With(s => s.Id, id ?? _fixture.Create<int>())
            .With(s => s.MusicalId, musicalId ?? _fixture.Create<int>())
            .With(s => s.Date, date ?? DateTime.Now.AddDays(7))
            .Without(s => s.Musical)
            .Without(s => s.CastMembers)
            .Create();

        return show;
    }

    public Role CreateRole(
        int? id = null,
        string? name = null,
        int? musicalId = null,
        RoleType roleType = RoleType.Main)
    {
        var role = _fixture.Build<Role>()
            .With(r => r.Id, id ?? _fixture.Create<int>())
            .With(r => r.Name, name ?? $"Role {_fixture.Create<int>()}")
            .With(r => r.MusicalId, musicalId ?? _fixture.Create<int>())
            .With(r => r.RoleType, roleType)
            .Without(r => r.ActorRoles)
            .Without(r => r.Musical)
            .Without(r => r.CastMembers)
            .Create();

        return role;
    }

    public Actor CreateActor(
        int? id = null,
        string? name = null,
        VoiceType voiceType = VoiceType.Tenor,
        Gender gender = Gender.Male)
    {
        var actor = _fixture.Build<Actor>()
            .With(a => a.Id, id ?? _fixture.Create<int>())
            .With(a => a.Name, name ?? $"Actor {_fixture.Create<int>()}")
            .With(a => a.VoiceType, voiceType)
            .With(a => a.Gender, gender)
            .With(a => a.BirthDate, new DateTime(1990, 1, 1))
            .With(a => a.Height, 170)
            .With(a => a.Weight, 70)
            .With(a => a.AddInfo, $"Additional info {_fixture.Create<int>()}")
            .Without(a => a.ActorRoles)
            .Without(a => a.CastMembers)
            .Create();

        return actor;
    }
}