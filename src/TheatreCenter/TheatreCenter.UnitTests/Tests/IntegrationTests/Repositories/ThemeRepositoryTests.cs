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
        // Act 1 - создать тему
        var theme = _themeFixture.CreateTheme();
        await _themeRepository.AddAsync(theme);

        // Assert 1 - проверить создание
        var created = await _themeRepository.GetByIdAsync(theme.Id);
        created.Should().NotBeNull();
        created!.Name.Should().Be(theme.Name);

        // Act 2 - обновить тему
        created.Name = theme.Name + "123";
        await _themeRepository.UpdateAsync(created);

        // Assert 2 - проверить обновление
        var updated = await _themeRepository.GetByIdAsync(theme.Id);
        updated!.Name.Should().Be(theme.Name);

        // Act 3 - получить все темы
        var allThemes = await _themeRepository.GetAllAsync();

        // Assert 3 - проверить наличие созданной темы
        allThemes.Should().Contain(t => t.Id == theme.Id);

        // Act 4 - удалить тему
        await _themeRepository.RemoveAsync(updated);

        // Assert 4 - проверить удаление
        var deleted = await _themeRepository.GetByIdAsync(theme.Id);
        deleted.Should().BeNull();
    }
}