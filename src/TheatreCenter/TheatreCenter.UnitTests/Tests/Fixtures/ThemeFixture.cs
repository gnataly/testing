using TheatreCenter.Domain.Models;
using AutoFixture;

namespace TheatreCenter.Tests.Fixtures;

public class ThemeFixture
{
    private readonly IFixture _fixture;

    public ThemeFixture()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _fixture.Customize<string>(c => c.FromFactory(() => Guid.NewGuid().ToString().Substring(0, 10)));
    }

    public Theme CreateTheme(
        int? id = null,
        string? name = null)
    {
        var theme = _fixture.Build<Theme>()
            .With(t => t.Id, id ?? _fixture.Create<int>())
            .With(t => t.Name, name ?? $"Theme {Guid.NewGuid()}")
            .Without(t => t.MusicalThemes)
            .Create();

        return theme;
    }
}