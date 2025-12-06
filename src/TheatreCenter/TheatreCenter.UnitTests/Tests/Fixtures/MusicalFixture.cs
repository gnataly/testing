using TheatreCenter.Domain.Models;
using TheatreCenter.Domain.Enums;
using AutoFixture;
using TheatreCenter.Data;

namespace TheatreCenter.Tests.Fixtures;

public class MusicalFixture
{
    private readonly IFixture _fixture;

    public MusicalFixture()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _fixture.Customize<string>(c => c.FromFactory(() =>
            Guid.NewGuid().ToString().Substring(0, 8)));
    }

    public Musical CreateMusical(
        int? id = null,
        string? title = null,
        string? description = null,
        TimeSpan? duration = null,
        AgeRestriction ageRestriction = AgeRestriction.EighteenPlus,
        int? theatreId = null)
    {
        var musicalTitle = title ?? $"M{_fixture.Create<int>() % 1000}";
        if (musicalTitle.Length > 100) musicalTitle = musicalTitle.Substring(0, 100);

        var musicalDescription = description ?? $"Desc{_fixture.Create<int>()}";

        var musical = _fixture.Build<Musical>()
            .With(m => m.Id, id ?? _fixture.Create<int>() + 5)
            .With(m => m.Title, title ?? $"Musical {_fixture.Create<int>()}")
            .With(m => m.Description, description ?? $"Description {_fixture.Create<int>()}")
            .With(m => m.Duration, duration ?? TimeSpan.FromHours(2))
            .With(m => m.AgeRestriction, ageRestriction)
            .With(m => m.TheatreId, theatreId ?? 1)
            .Without(m => m.Theatre)
            .Without(m => m.Shows)
            .Without(m => m.Roles)
            .Without(m => m.MusicalThemes)
            .Create();

        return musical;
    }

    public Theatre CreateTheatre(int? id = null, string? name = null, string? addInfo = null)
    {
        var theatreName = name ?? $"T{_fixture.Create<int>() % 1000}";
        if (theatreName.Length > 100) theatreName = theatreName.Substring(0, 100);

        var theatreAddInfo = addInfo ?? $"Info{_fixture.Create<int>()}";

        var theatre = _fixture.Build<Theatre>()
            .With(t => t.Id, id ?? _fixture.Create<int>() + 5)
            .With(t => t.Name, name ?? $"Theatre {_fixture.Create<int>()}")
            .With(t => t.AddInfo, addInfo ?? $"Additional info {_fixture.Create<int>()}")
            .Without(t => t.Musicals)
            .Create();

        return theatre;
    }
}
