using TheatreCenter.Domain.Models;
using TheatreCenter.Domain.Enums;
using AutoFixture;

namespace TheatreCenter.Tests.Fixtures;

public class ActorFixture
{
    private readonly IFixture _fixture;

    public ActorFixture()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _fixture.Customize<string>(c => c.FromFactory(() =>
            Guid.NewGuid().ToString().Substring(0, 8)));
    }

    public Actor CreateActor(
        int? id = null,
        string? name = null,
        VoiceType voiceType = VoiceType.Tenor,
        Gender gender = Gender.Male,
        DateTime? birthDate = null,
        int? height = null,
        int? weight = null,
        string? addInfo = null)
    {
        var actorName = name ?? $"A{_fixture.Create<int>() % 1000}";
        if (actorName.Length > 100) actorName = actorName.Substring(0, 100);

        var actorAddInfo = addInfo ?? $"Info{_fixture.Create<int>()}";

        var actor = _fixture.Build<Actor>()
            .With(a => a.Id, id ?? _fixture.Create<int>() + 5)
            .With(a => a.Name, name ?? $"Actor {_fixture.Create<int>()}")
            .With(a => a.VoiceType, voiceType)
            .With(a => a.Gender, gender)
            .With(a => a.BirthDate, birthDate ?? new DateTime(1990, 1, 1))
            .With(a => a.Height, height ?? 170)
            .With(a => a.Weight, weight ?? 70)
            .With(a => a.AddInfo, addInfo ?? $"Additional info {_fixture.Create<int>()}")
            .Without(a => a.ActorRoles)
            .Without(a => a.CastMembers)
            .Create();

        return actor;
    }
}