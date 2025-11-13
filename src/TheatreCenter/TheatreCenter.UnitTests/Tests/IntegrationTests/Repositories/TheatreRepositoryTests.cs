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
    private TheatreRepository _repository;
    private Func<Task> _commitTransaction;
    private Func<Task> _rollbackTransaction;

    public TheatreRepositoryIt(DatabaseFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    public override async Task InitializeAsync()
    {
        var (repository, commit, rollback) = await Fixture.CreateTransactionalRepositoryAsync<TheatreRepository>();
        _repository = repository;
        _commitTransaction = commit;
        _rollbackTransaction = rollback;
    }

    public override async Task DisposeAsync()
    {
        await _rollbackTransaction();
    }

    [Fact]
    public async Task Theatre_FullCycle_WithMusicals()
    {

        // Act 1 - создать театр
        var theatre = _theatreFixture.CreateTheatre();

        await _repository.AddAsync(theatre);

        // Assert 1 - проверить создание
        var created = await _repository.GetByIdAsync(theatre.Id);
        created.Should().NotBeNull();
        created!.Name.Should().Be(theatre.Name);
        created.AddInfo.Should().Be(theatre.AddInfo);

        // Act 2 - обновить театр
        created.Name = theatre.Name + "123";
        created.AddInfo = theatre.AddInfo + "123";
        await _repository.UpdateAsync(created);

        // Assert 2 - проверить обновление
        var updated = await _repository.GetByIdAsync(theatre.Id);
        updated!.Name.Should().Be(theatre.Name);
        updated.AddInfo.Should().Be(theatre.AddInfo);

        // Act 3 - получить все театры
        var allTheatres = await _repository.GetAllAsync();

        // Assert 3 - проверить получение всех театров
        allTheatres.Should().Contain(t => t.Id == theatre.Id);

        // Act 4 - удалить театр
        await _repository.RemoveAsync(updated);

        // Assert 4 - проверить удаление
        var deleted = await _repository.GetByIdAsync(theatre.Id);
        deleted.Should().BeNull();
    }
}