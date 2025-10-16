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
    }

    public Show CreateShow(
        int? id = null,
        int? musicalId = null,
        DateTime? date = null,
        bool futureDate = true)
    {
        var showDate = date ?? (futureDate
            ? DateTime.Now.AddDays(7)
            : DateTime.Now.AddDays(-7));

        var show = _fixture.Build<Show>()
            .With(s => s.Id, id ?? _fixture.Create<int>())
            .With(s => s.MusicalId, musicalId ?? _fixture.Create<int>())
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
        var musical = _fixture.Build<Musical>()
            .With(m => m.Id, id ?? _fixture.Create<int>())
            .With(m => m.Title, title ?? $"Musical {_fixture.Create<int>()}")
            .With(m => m.Description, $"Description {_fixture.Create<int>()}")
            .With(m => m.Duration, TimeSpan.FromHours(2))
            .With(m => m.AgeRestriction, ageRestriction)
            .With(m => m.TheatreId, _fixture.Create<int>())
            .Without(m => m.Theatre)
            .Without(m => m.Shows)
            .Without(m => m.Roles)
            .Without(m => m.MusicalThemes)
            .Create();

        return musical;
    }
}