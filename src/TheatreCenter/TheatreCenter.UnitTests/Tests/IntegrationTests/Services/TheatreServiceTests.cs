using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TheatreCenter.Data.Repositories;
using TheatreCenter.Data;
using TheatreCenter.Services.Services;
using TheatreCenter.Tests.Fixtures;
using TheatreCenter.UnitTests.Tests.Database;
using Xunit;
using FluentAssertions;
using Xunit.Abstractions;
using TheatreCenter.UnitTests;

namespace TheatreCenter.UnitTests.Tests.IntegrationTests.Services;

[Collection("Database collection")]
[Trait("Category", TestCategories.Integration)]
public class TheatreServiceIt : IntegrationTestBase
{
    private readonly TheatreFixture _theatreFixture = new TheatreFixture();
    private TheatreService _service;
    private Func<Task> _commitTransaction;
    private Func<Task> _rollbackTransaction;

    public TheatreServiceIt(DatabaseFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    public override async Task InitializeAsync()
    {
        var context = await Fixture.CreateTransactionalContextAsync();
        var repository = Fixture.CreateRepository<TheatreRepository>(context);
        _service = new TheatreService(repository);

        _commitTransaction = async () => {
            await context.Database.CommitTransactionAsync();
            await context.DisposeAsync();
        };
        _rollbackTransaction = async () => {
            await context.Database.RollbackTransactionAsync();
            await context.DisposeAsync();
        };
    }

    public override async Task DisposeAsync()
    {
        await _rollbackTransaction();
    }

    [Fact]
    public async Task Theatre_FullCycle_WithFixtures()
    {
        // Используем фикстуру для генерации театра
        var testTheatre = _theatreFixture.CreateTheatre(
            name: "Test Theatre",
            addInfo: "Test additional info"
        );

        // Act 1 — создание театра
        var createdTheatre = await _service.CreateTheatreAsync(testTheatre);

        // Assert 1 — проверка создания
        createdTheatre.Should().NotBeNull();
        createdTheatre.Id.Should().BeGreaterThan(0);
        createdTheatre.Name.Should().Be(testTheatre.Name);

        // Act 2 — получение театра по ID
        var retrievedTheatre = await _service.GetTheatreByIdAsync(createdTheatre.Id);
        retrievedTheatre.Should().NotBeNull();
        retrievedTheatre.Name.Should().Be(testTheatre.Name);

        // Act 3 — обновление театра
        //var updatedTheatre = _theatreFixture.CreateTheatre(
        //    id: createdTheatre.Id,
        //    name: "Updated Theatre Name",
        //    addInfo: "Updated additional info"
        //);

        createdTheatre.Name = "Updated Theatre Name";

        var updateResult = await _service.UpdateTheatreAsync(createdTheatre);
        updateResult.Should().NotBeNull();
        updateResult.Name.Should().Be("Updated Theatre Name");

        // Act 4 — получение всех театров
        var allTheatres = await _service.GetAllTheatresAsync();
        allTheatres.Should().Contain(t => t.Id == createdTheatre.Id);

        // Act 5 — удаление театра
        var deleteResult = await _service.DeleteTheatreAsync(createdTheatre.Id);
        deleteResult.Should().BeTrue();
    }
}