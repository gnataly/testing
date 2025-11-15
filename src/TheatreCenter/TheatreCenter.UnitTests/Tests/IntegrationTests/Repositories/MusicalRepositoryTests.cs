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
        var theatre = _theatreFixture.CreateTheatre();
        await _theatreRepository.AddAsync(theatre);

        var musical = _musicalFixture.CreateMusical(
            ageRestriction: AgeRestriction.SixteenPlus,
            theatreId: theatre.Id);

        await _musicalRepository.AddAsync(musical);

        var created = await _musicalRepository.GetByIdAsync(musical.Id);
        created.Should().NotBeNull();
        created!.Title.Should().Be(musical.Title);
        created.AgeRestriction.Should().Be(AgeRestriction.SixteenPlus);
        created.TheatreId.Should().Be(musical.TheatreId);

        created.Title = musical.Title + "123";
        created.AgeRestriction = AgeRestriction.EighteenPlus;
        await _musicalRepository.UpdateAsync(created);

        var updated = await _musicalRepository.GetByIdAsync(musical.Id);
        updated!.Title.Should().Be(musical.Title);
        updated.AgeRestriction.Should().Be(AgeRestriction.EighteenPlus);

        var theatreMusicals = await _musicalRepository.GetByTheatreIdAsync(musical.TheatreId);

        theatreMusicals.Should().ContainSingle(m => m.Id == musical.Id);

        var adultMusicals = await _musicalRepository.GetByAgeRestrictionAsync(AgeRestriction.EighteenPlus);

        adultMusicals.Should().ContainSingle(m => m.Id == musical.Id);

        await _musicalRepository.RemoveAsync(updated);

        var deleted = await _musicalRepository.GetByIdAsync(musical.Id);
        deleted.Should().BeNull();
    }
}