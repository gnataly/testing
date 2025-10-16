using TheatreCenter.Domain.Models;
using AutoFixture;

namespace TheatreCenter.Tests.Fixtures;

public class TheatreFixture
{
    private readonly IFixture _fixture;

    public TheatreFixture()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _fixture.Customize<string>(c => c.FromFactory(() => Guid.NewGuid().ToString().Substring(0, 10)));
    }

    public Theatre CreateTheatre(
        int? id = null,
        string? name = null,
        string? addInfo = null)
    {
        var theatre = _fixture.Build<Theatre>()
            .With(t => t.Id, id ?? _fixture.Create<int>())
            .With(t => t.Name, name ?? $"Theatre {_fixture.Create<int>()}")
            .With(t => t.AddInfo, addInfo ?? $"Additional info {_fixture.Create<int>()}")
            .Without(t => t.Musicals)
            .Create();

        return theatre;
    }
}