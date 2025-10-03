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

        // Настраиваем автоматическую генерацию для строковых свойств
        _fixture.Customize<string>(c => c.FromFactory(() => Guid.NewGuid().ToString().Substring(0, 10)));
    }

    public Theatre CreateTheatre(
        int? id = null,
        string? name = null)
    {
        var theatre = _fixture.Build<Theatre>()
            .With(t => t.Id, id ?? _fixture.Create<int>())
            .With(t => t.Name, name ?? $"Test Theatre {Guid.NewGuid().ToString().Substring(0, 8)}")
            .Create();

        return theatre;
    }
}