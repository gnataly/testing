using Microsoft.EntityFrameworkCore;
using TheatreCenter.Data;
using TheatreCenter.Data.Repositories;
using TheatreCenter.Domain.Models;
using TheatreCenter.Tests.Fixtures;
using FluentAssertions;
using TheatreCenter.UnitTests.Tests.Database;
using TheatreCenter.UnitTests;
using Xunit;
using Xunit.Abstractions;
using AutoFixture;
using TheatreCenter.UnitTests.Tests.IntegrationTests;

namespace TheatreCenter.Tests.IntegrationTests.Repositories;

[Collection("Database collection")]
[Trait("Category", TestCategories.Integration)]
public class ThemeRepositoryIt : IntegrationTestBase
{
    private readonly ThemeFixture _themeFixture = new ThemeFixture();
    private ThemeRepository _themeRepository;
    private Func<Task> _commitTransaction;
    private Func<Task> _rollbackTransaction;

    public ThemeRepositoryIt(DatabaseFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    public override async Task InitializeAsync()
    {
        var context = await Fixture.CreateTransactionalContextAsync();

        _themeRepository = Fixture.CreateRepository<ThemeRepository>(context);

        _commitTransaction = async () => { await context.Database.CommitTransactionAsync(); await context.DisposeAsync(); };
        _rollbackTransaction = async () => { await context.Database.RollbackTransactionAsync(); await context.DisposeAsync(); };
    }

    public override async Task DisposeAsync()
    {
        await _rollbackTransaction();
    }

    [Fact]
    public async Task Theme_FullCycle()
    {
        var theme = _themeFixture.CreateTheme();
        await _themeRepository.AddAsync(theme);

        var created = await _themeRepository.GetByIdAsync(theme.Id);
        created.Should().NotBeNull();
        created!.Name.Should().Be(theme.Name);

        created.Name = theme.Name + "123";
        await _themeRepository.UpdateAsync(created);

        var updated = await _themeRepository.GetByIdAsync(theme.Id);
        updated!.Name.Should().Be(theme.Name);

        var allThemes = await _themeRepository.GetAllAsync();

        allThemes.Should().Contain(t => t.Id == theme.Id);

        await _themeRepository.RemoveAsync(updated);

        var deleted = await _themeRepository.GetByIdAsync(theme.Id);
        deleted.Should().BeNull();
    }
}