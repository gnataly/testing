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
public class TheatreRepositoryIt : IntegrationTestBase
{
    private readonly TheatreFixture _theatreFixture = new TheatreFixture();
    private TheatreRepository _theatreRepository;
    private Func<Task> _commitTransaction;
    private Func<Task> _rollbackTransaction;

    public TheatreRepositoryIt(DatabaseFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    public override async Task InitializeAsync()
    {
        var context = await Fixture.CreateTransactionalContextAsync();

        _theatreRepository = Fixture.CreateRepository<TheatreRepository>(context);

        _commitTransaction = async () => { await context.Database.CommitTransactionAsync(); await context.DisposeAsync(); };
        _rollbackTransaction = async () => { await context.Database.RollbackTransactionAsync(); await context.DisposeAsync(); };
    }

    public override async Task DisposeAsync()
    {
        await _rollbackTransaction();
    }

    [Fact]
    public async Task Theatre_FullCycle_WithMusicals()
    {
        var theatre = _theatreFixture.CreateTheatre();
        await _theatreRepository.AddAsync(theatre);

        var created = await _theatreRepository.GetByIdAsync(theatre.Id);
        created.Should().NotBeNull();
        created!.Name.Should().Be(theatre.Name);
        created.AddInfo.Should().Be(theatre.AddInfo);

        created.Name = theatre.Name + "123";
        created.AddInfo = theatre.AddInfo + "123";
        await _theatreRepository.UpdateAsync(created);

        var updated = await _theatreRepository.GetByIdAsync(theatre.Id);
        updated!.Name.Should().Be(theatre.Name);
        updated.AddInfo.Should().Be(theatre.AddInfo);

        var allTheatres = await _theatreRepository.GetAllAsync();

        allTheatres.Should().Contain(t => t.Id == theatre.Id);
        
        await _theatreRepository.RemoveAsync(updated);

        var deleted = await _theatreRepository.GetByIdAsync(theatre.Id);
        deleted.Should().BeNull();
    }
}