using TheatreCenter.Data;
using TheatreCenter.Domain.Models;
using TheatreCenter.Domain.Enums;
using TheatreCenter.Services.Services;
using TheatreCenter.Tests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TheatreCenter.Data.Repositories;
using TheatreCenter.UnitTests.Tests.Database;
using TheatreCenter.UnitTests;
using Xunit;
using Xunit.Abstractions;
using AutoFixture;
using TheatreCenter.UnitTests.Tests.IntegrationTests;
using System.Diagnostics;

namespace TheatreCenter.Tests.IntegrationTests.Services;

[CollectionDefinition("Database collection")]
[Trait("Category", TestCategories.Integration)]
public class MusicalServiceIt : IntegrationTestBase
{
    private readonly MusicalFixture _musicalFixture = new MusicalFixture();
    private readonly TheatreFixture _theatreFixture = new TheatreFixture();
    private TheatreRepository _theatreRepository;
    private MusicalService _service;
    private Func<Task> _commitTransaction;
    private Func<Task> _rollbackTransaction;
    private readonly ITestOutputHelper _outputHelper;

    public MusicalServiceIt(DatabaseFixture fixture, ITestOutputHelper output)
        : base(fixture, output) {
        _outputHelper = output;
    }

    public override async Task InitializeAsync()
    {
        //await Fixture.WaitForDatabaseReadyAsync(TimeSpan.FromSeconds(30));
        var context = await Fixture.CreateTransactionalContextAsync();
        var musicalRepository = Fixture.CreateRepository<MusicalRepository>(context);
        _theatreRepository = Fixture.CreateRepository<TheatreRepository>(context);
        _service = new MusicalService(musicalRepository, _theatreRepository);

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
    public async Task Musical_FullCycle_WithFixtures()
    {
        //_outputHelper.WriteLine("\n\n\n\n\nTEST");
        //var context = await Fixture.CreateTransactionalContextAsync();
        //// Создаем театр для мюзикла
        var theatre = _theatreFixture.CreateTheatre();
        await _theatreRepository.AddAsync(theatre);

        //await context.Theatres.AddAsync(theatre);
        //await context.SaveChangesAsync();
        //await context.Database.CommitTransactionAsync();

        // Используем фикстуру для генерации мюзикла
        var testMusical = _musicalFixture.CreateMusical(
            duration: TimeSpan.FromHours(2),
            ageRestriction: AgeRestriction.TwelvePlus,
            theatreId: theatre.Id
        );

        // Act 1 — создание мюзикла
        var createdMusical = await _service.CreateMusicalAsync(testMusical);

        // Assert 1 — проверка создания
        createdMusical.Should().NotBeNull();
        createdMusical.Id.Should().BeGreaterThan(0);
        createdMusical.Title.Should().Be(testMusical.Title);

        // Act 2 — получение мюзикла по ID
        var retrievedMusical = await _service.GetMusicalByIdAsync(createdMusical.Id);
        retrievedMusical.Should().NotBeNull();
        retrievedMusical.Title.Should().Be(testMusical.Title);

        // Act 3 — обновление мюзикла
        var updatedMusical = _musicalFixture.CreateMusical(
            id: createdMusical.Id,
            ageRestriction: createdMusical.AgeRestriction,
            theatreId: testMusical.TheatreId
        );

        var updateResult = await _service.UpdateMusicalAsync(updatedMusical);
        updateResult.Should().NotBeNull();
        updateResult.Title.Should().Be(updatedMusical.Title);

        // Act 4 — получение мюзиклов по театру
        var musicalsByTheatre = await _service.GetMusicalsByTheatreAsync(testMusical.TheatreId);
        musicalsByTheatre.Should().Contain(m => m.Id == createdMusical.Id);

        // Act 5 — получение мюзиклов по возрастному ограничению
        var musicalsByAge = await _service.GetMusicalsByAgeRestrictionAsync(AgeRestriction.TwelvePlus);
        musicalsByAge.Should().Contain(m => m.Id == createdMusical.Id);

        // Act 6 — получение всех мюзиклов
        var allMusicals = await _service.GetAllMusicalsAsync();
        allMusicals.Should().Contain(m => m.Id == createdMusical.Id);

        // Act 7 — удаление мюзикла
        var deleteResult = await _service.DeleteMusicalAsync(createdMusical.Id);
        deleteResult.Should().BeTrue();
    }
}