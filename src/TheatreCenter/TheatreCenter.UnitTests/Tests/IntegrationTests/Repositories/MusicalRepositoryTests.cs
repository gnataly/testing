using Microsoft.EntityFrameworkCore;
using TheatreCenter.Data;
using TheatreCenter.Data.Repositories;
using TheatreCenter.Domain.Enums;
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
public class MusicalRepositoryIt : IntegrationTestBase
{
    private readonly MusicalFixture _musicalFixture = new MusicalFixture();
    private readonly TheatreFixture _theatreFixture = new TheatreFixture();
    private MusicalRepository _musicalRepository;
    private TheatreRepository _theatreRepository;
    private Func<Task> _commitTransaction;
    private Func<Task> _rollbackTransaction;

    public MusicalRepositoryIt(DatabaseFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    public override async Task InitializeAsync()
    {
        var context = await Fixture.CreateTransactionalContextAsync();

        _musicalRepository = Fixture.CreateRepository<MusicalRepository>(context);
        _theatreRepository = Fixture.CreateRepository<TheatreRepository>(context);

        _commitTransaction = async () => { await context.Database.CommitTransactionAsync(); await context.DisposeAsync(); };
        _rollbackTransaction = async () => { await context.Database.RollbackTransactionAsync(); await context.DisposeAsync(); };
    }

    public override async Task DisposeAsync()
    {
        await _rollbackTransaction();
    }

    [Fact]
    public async Task Musical_FullCycle_WithAgeRestriction()
    {
        // Arrange - создаем театр
        var theatre = _theatreFixture.CreateTheatre();
        await _theatreRepository.AddAsync(theatre);

        // Act 1 - создать мюзикл
        var musical = _musicalFixture.CreateMusical(
            ageRestriction: AgeRestriction.SixteenPlus,
            theatreId: theatre.Id);

        await _musicalRepository.AddAsync(musical);

        // Assert 1 - проверить создание
        var created = await _musicalRepository.GetByIdAsync(musical.Id);
        created.Should().NotBeNull();
        created!.Title.Should().Be(musical.Title);
        created.AgeRestriction.Should().Be(AgeRestriction.SixteenPlus);
        created.TheatreId.Should().Be(musical.TheatreId);

        // Act 2 - обновить мюзикл
        created.Title = musical.Title + "123";
        created.AgeRestriction = AgeRestriction.EighteenPlus;
        await _musicalRepository.UpdateAsync(created);

        // Assert 2 - проверить обновление
        var updated = await _musicalRepository.GetByIdAsync(musical.Id);
        updated!.Title.Should().Be(musical.Title);
        updated.AgeRestriction.Should().Be(AgeRestriction.EighteenPlus);

        // Act 3 - получить мюзиклы по театру
        var theatreMusicals = await _musicalRepository.GetByTheatreIdAsync(musical.TheatreId);

        // Assert 3 - проверить фильтрацию по театру
        theatreMusicals.Should().ContainSingle(m => m.Id == musical.Id);

        // Act 4 - получить мюзиклы по возрастному ограничению
        var adultMusicals = await _musicalRepository.GetByAgeRestrictionAsync(AgeRestriction.EighteenPlus);

        // Assert 4 - проверить фильтрацию по возрастному ограничению
        adultMusicals.Should().ContainSingle(m => m.Id == musical.Id);

        // Act 5 - удалить мюзикл
        await _musicalRepository.RemoveAsync(updated);

        // Assert 5 - проверить удаление
        var deleted = await _musicalRepository.GetByIdAsync(musical.Id);
        deleted.Should().BeNull();
    }
}