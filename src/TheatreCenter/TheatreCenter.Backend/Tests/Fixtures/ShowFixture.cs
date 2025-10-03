using TheatreCenter.Domain.Models;
using AutoFixture;

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
        bool futureDate = true)
    {
        var date = futureDate
            ? DateTime.Now.AddDays(7)
            : DateTime.Now.AddDays(-7);

        var show = _fixture.Build<Show>()
            .With(s => s.Id, id ?? _fixture.Create<int>())
            .With(s => s.MusicalId, musicalId ?? _fixture.Create<int>())
            .With(s => s.Date, date)
            .Without(s => s.Musical)
            .Without(s => s.CastMembers)
            .Create();

        return show;
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