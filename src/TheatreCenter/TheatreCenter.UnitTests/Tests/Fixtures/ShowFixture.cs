using TheatreCenter.Domain.Models;
using AutoFixture;
using TheatreCenter.Domain.Enums;

namespace TheatreCenter.Tests.Fixtures;

public class ShowFixture
{
    private readonly IFixture _fixture;

    public ShowFixture()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _fixture.Customize<string>(c => c.FromFactory(() =>
            Guid.NewGuid().ToString().Substring(0, 8)));
    }

    public Show CreateShow(
        int? id = null,
        int? musicalId = null,
        DateTime? date = null,
        bool futureDate = true)
    {
        var showDate = date ?? (futureDate
            ? DateTime.UtcNow.AddDays(7)
            : DateTime.UtcNow.AddDays(-7));

        var show = _fixture.Build<Show>()
            .With(s => s.Id, id ?? _fixture.Create<int>())
            .With(s => s.MusicalId, musicalId ?? 1)
            .With(s => s.Date, showDate)
            .Without(s => s.Musical)
            .Without(s => s.CastMembers)
            .Create();

        return show;
    }

    public Musical CreateMusical(
        int? id = null,
        string? title = null,
        AgeRestriction ageRestriction = AgeRestriction.EighteenPlus)
    {
        var musicalTitle = title ?? $"M{_fixture.Create<int>() % 1000}";
        if (musicalTitle.Length > 100) musicalTitle = musicalTitle.Substring(0, 100);

        var musical = _fixture.Build<Musical>()
            .With(m => m.Id, id ?? _fixture.Create<int>())
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