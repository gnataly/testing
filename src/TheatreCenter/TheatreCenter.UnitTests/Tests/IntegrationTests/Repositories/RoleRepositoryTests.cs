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
public class RoleRepositoryIt : IntegrationTestBase
{
    private readonly RoleFixture _roleFixture = new RoleFixture();
    private readonly MusicalFixture _musicalFixture = new MusicalFixture();
    private RoleRepository _repository;
    private Func<Task> _commitTransaction;
    private Func<Task> _rollbackTransaction;

    public RoleRepositoryIt(DatabaseFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    public override async Task InitializeAsync()
    {
        var (repository, commit, rollback) = await Fixture.CreateTransactionalRepositoryAsync<RoleRepository>();
        _repository = repository;
        _commitTransaction = commit;
        _rollbackTransaction = rollback;
    }

    public override async Task DisposeAsync()
    {
        await _rollbackTransaction();
    }

    [Fact]
    public async Task Role_FullCycle_WithRoleType()
    {
        // Arrange - Создаем мюзикл для роли
        var musical = _musicalFixture.CreateMusical(title: "Test Musical");

        var context = await Fixture.CreateTransactionalContextAsync();
        context.Musicals.Add(musical);
        await context.SaveChangesAsync();
        await context.Database.CommitTransactionAsync();
        await context.DisposeAsync();

        // Act 1 - создать роль
        var role = _roleFixture.CreateRole(
            name: "Test Role",
            roleType: RoleType.Main,
            musicalId: musical.Id);

        await _repository.AddAsync(role);

        // Assert 1 - проверить создание
        var created = await _repository.GetByIdAsync(role.Id);
        created.Should().NotBeNull();
        created!.Name.Should().Be("Test Role");
        created.RoleType.Should().Be(RoleType.Main);
        created.MusicalId.Should().Be(musical.Id);

        // Act 2 - обновить роль
        created.Name = "Updated Role";
        created.RoleType = RoleType.Supporting;
        await _repository.UpdateAsync(created);

        // Assert 2 - проверить обновление
        var updated = await _repository.GetByIdAsync(role.Id);
        updated!.Name.Should().Be("Updated Role");
        updated.RoleType.Should().Be(RoleType.Supporting);

        // Act 3 - получить роли по мюзиклу
        var musicalRoles = await _repository.GetByMusicalIdAsync(musical.Id);

        // Assert 3 - проверить фильтрацию по мюзиклу
        musicalRoles.Should().ContainSingle(r => r.Id == role.Id);

        // Act 4 - получить роли по типу
        var supportingRoles = await _repository.GetByRoleTypeAsync(RoleType.Supporting);

        // Assert 4 - проверить фильтрацию по типу роли
        supportingRoles.Should().ContainSingle(r => r.Id == role.Id);

        // Act 5 - удалить роль
        await _repository.RemoveAsync(updated);

        // Assert 5 - проверить удаление
        var deleted = await _repository.GetByIdAsync(role.Id);
        deleted.Should().BeNull();
    }
}