using TheatreCenter.Domain.Models;
using TheatreCenter.Domain.Enums;
using AutoFixture;

namespace TheatreCenter.Tests.Fixtures;

public class MusicalFixture
{
    private readonly IFixture _fixture;

    public MusicalFixture()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _fixture.Customize<string>(c => c.FromFactory(() => Guid.NewGuid().ToString().Substring(0, 10)));
    }

    public Musical CreateMusical(
        int? id = null,
        string? title = null,
        string? description = null,
        TimeSpan? duration = null,
        AgeRestriction ageRestriction = AgeRestriction.EighteenPlus,
        int? theatreId = null)
    {
        var musical = _fixture.Build<Musical>()
            .With(m => m.Id, id ?? _fixture.Create<int>())
            .With(m => m.Title, title ?? $"Musical {_fixture.Create<int>()}")
            .With(m => m.Description, description ?? $"Description {_fixture.Create<int>()}")
            .With(m => m.Duration, duration ?? TimeSpan.FromHours(2))
            .With(m => m.AgeRestriction, ageRestriction)
            .With(m => m.TheatreId, theatreId ?? _fixture.Create<int>())
            .Without(m => m.Shows)
            .Without(m => m.Roles)
            .Create();

        return musical;
    }

    public Theatre CreateTheatre(int? id = null, string? name = null)
    {
        var theatre = _fixture.Build<Theatre>()
            .With(t => t.Id, id ?? _fixture.Create<int>())
            .With(t => t.Name, name ?? $"Theatre {_fixture.Create<int>()}")
            .Without(t => t.Musicals)
            .Create();

        return theatre;
    }
}