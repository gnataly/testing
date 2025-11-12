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

namespace TheatreCenter.Tests.IntegrationTests.Repositories;

[Collection("Database collection")]
[Trait("Category", TestCategories.Integration)]
public class RoleRepositoryIt(DatabaseFixture db)
    : IClassFixture<DatabaseFixture>
{
    private readonly RoleFixture _roleFixture = new RoleFixture();
    private readonly MusicalFixture _musicalFixture = new MusicalFixture();

    private RoleRepository CreateRepository()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;

        var context = new AppDbContext(options);
        return new RoleRepository(context);
    }

    [Fact]
    public async Task Role_FullCycle_WithRoleType()
    {
        // Arrange
        var repo = CreateRepository();

        // Создаем мюзикл для роли
        var musical = _musicalFixture.CreateMusical(title: "Test Musical");

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;
        using var context = new AppDbContext(options);
        context.Musicals.Add(musical);
        await context.SaveChangesAsync();

        // Act 1 - создать роль
        var role = _roleFixture.CreateRole(
            name: "Test Role",
            roleType: RoleType.Main,
            musicalId: musical.Id);

        await repo.AddAsync(role);

        // Assert 1 - проверить создание
        var created = await repo.GetByIdAsync(role.Id);
        created.Should().NotBeNull();
        created!.Name.Should().Be("Test Role");
        created.RoleType.Should().Be(RoleType.Main);
        created.MusicalId.Should().Be(musical.Id);

        // Act 2 - обновить роль
        created.Name = "Updated Role";
        created.RoleType = RoleType.Supporting;
        await repo.UpdateAsync(created);

        // Assert 2 - проверить обновление
        var updated = await repo.GetByIdAsync(role.Id);
        updated!.Name.Should().Be("Updated Role");
        updated.RoleType.Should().Be(RoleType.Supporting);

        // Act 3 - получить роли по мюзиклу
        var musicalRoles = await repo.GetByMusicalIdAsync(musical.Id);

        // Assert 3 - проверить фильтрацию по мюзиклу
        musicalRoles.Should().ContainSingle(r => r.Id == role.Id);

        // Act 4 - получить роли по типу
        var supportingRoles = await repo.GetByRoleTypeAsync(RoleType.Supporting);

        // Assert 4 - проверить фильтрацию по типу роли
        supportingRoles.Should().ContainSingle(r => r.Id == role.Id);

        // Act 5 - удалить роль
        await repo.RemoveAsync(updated);

        // Assert 5 - проверить удаление
        var deleted = await repo.GetByIdAsync(role.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task Role_GetByRoleType_ReturnsCorrectRoles()
    {
        // Arrange
        var repo = CreateRepository();

        var musical = _musicalFixture.CreateMusical();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;
        using var context = new AppDbContext(options);
        context.Musicals.Add(musical);
        await context.SaveChangesAsync();

        var mainRole = _roleFixture.CreateRole(
            name: "Main Role",
            roleType: RoleType.Main,
            musicalId: musical.Id);

        var supportingRole = _roleFixture.CreateRole(
            name: "Supporting Role",
            roleType: RoleType.Supporting,
            musicalId: musical.Id);

        await repo.AddAsync(mainRole);
        await repo.AddAsync(supportingRole);

        // Act
        var mainRoles = await repo.GetByRoleTypeAsync(RoleType.Main);
        var supportingRoles = await repo.GetByRoleTypeAsync(RoleType.Supporting);

        // Assert
        mainRoles.Should().ContainSingle(r => r.Name == "Main Role");
        supportingRoles.Should().ContainSingle(r => r.Name == "Supporting Role");

        // Cleanup
        await repo.RemoveAsync(mainRole);
        await repo.RemoveAsync(supportingRole);
    }
}