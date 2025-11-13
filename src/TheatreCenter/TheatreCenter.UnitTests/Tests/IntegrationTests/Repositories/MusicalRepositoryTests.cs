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
    private MusicalRepository _repository;
    private Func<Task> _commitTransaction;
    private Func<Task> _rollbackTransaction;

    public MusicalRepositoryIt(DatabaseFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    public override async Task InitializeAsync()
    {
        var (repository, commit, rollback) = await Fixture.CreateTransactionalRepositoryAsync<MusicalRepository>();
        _repository = repository;
        _commitTransaction = commit;
        _rollbackTransaction = rollback;
    }

    public override async Task DisposeAsync()
    {
        await _rollbackTransaction();
    }

    [Fact]
    public async Task Musical_FullCycle_WithAgeRestriction()
    {
        var context = await Fixture.CreateTransactionalContextAsync();
        var theatre = _theatreFixture.CreateTheatre();

        context.Theatres.Add(theatre);
        await context.SaveChangesAsync();
        await context.Database.CommitTransactionAsync();

        // Act 1 - создать мюзикл
        var musical = _musicalFixture.CreateMusical(
            ageRestriction: AgeRestriction.SixteenPlus,
            theatreId: theatre.Id);

        await _repository.AddAsync(musical);

        // Assert 1 - проверить создание
        var created = await _repository.GetByIdAsync(musical.Id);
        created.Should().NotBeNull();
        created!.Title.Should().Be(musical.Title);
        created.AgeRestriction.Should().Be(AgeRestriction.SixteenPlus);
        created.TheatreId.Should().Be(theatre.Id);

        // Act 2 - обновить мюзикл
        created.Title = musical.Title + "123";
        created.AgeRestriction = AgeRestriction.EighteenPlus;
        await _repository.UpdateAsync(created);

        // Assert 2 - проверить обновление
        var updated = await _repository.GetByIdAsync(musical.Id);
        updated!.Title.Should().Be(musical.Title);
        updated.AgeRestriction.Should().Be(AgeRestriction.EighteenPlus);

        // Act 3 - получить мюзиклы по театру
        var theatreMusicals = await _repository.GetByTheatreIdAsync(theatre.Id);

        // Assert 3 - проверить фильтрацию по театру
        theatreMusicals.Should().ContainSingle(m => m.Id == musical.Id);

        // Act 4 - получить мюзиклы по возрастному ограничению
        var adultMusicals = await _repository.GetByAgeRestrictionAsync(AgeRestriction.EighteenPlus);

        // Assert 4 - проверить фильтрацию по возрастному ограничению
        adultMusicals.Should().ContainSingle(m => m.Id == musical.Id);

        // Act 5 - удалить мюзикл
        await _repository.RemoveAsync(updated);

        // Assert 5 - проверить удаление
        var deleted = await _repository.GetByIdAsync(musical.Id);
        deleted.Should().BeNull();

    }
}