using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheatreCenter.Data.Repositories;
using TheatreCenter.Data;
using TheatreCenter.Domain.Enums;
using TheatreCenter.Services.Services;
using TheatreCenter.Tests.Fixtures;
using TheatreCenter.UnitTests.Tests.Database;
using Xunit;
using FluentAssertions;

namespace TheatreCenter.UnitTests.Tests.IntegrationTests.Services;

[Collection("Database collection")]
[Trait("Category", TestCategories.Integration)]
public class RoleServiceIt(DatabaseFixture db) : IClassFixture<DatabaseFixture>
{
    private readonly RoleFixture _roleFixture = new RoleFixture();
    private readonly MusicalFixture _musicalFixture = new MusicalFixture();

    private RoleService CreateService()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;

        var context = new AppDbContext(options);
        var roleRepository = new RoleRepository(context);
        var musicalRepository = new MusicalRepository(context);
        return new RoleService(roleRepository, musicalRepository);
    }

    [Fact]
    public async Task Role_FullCycle_WithFixtures()
    {
        //using (StreamWriter writer = new StreamWriter(@"C:\Users\gnata\OneDrive\Документы\log.txt"))
        //{
            var service = CreateService();

            // Создаем театр и мюзикл для роли
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(db.ConnectionString)
                .Options;

            var musical = _musicalFixture.CreateMusical(
                title: "Test Musical"
            );

            await using (var context = new AppDbContext(options))
            {
                await context.Musicals.AddAsync(musical);
                await context.SaveChangesAsync();
            }

            // Используем фикстуру для генерации роли
            var testRole = _roleFixture.CreateRole(
                name: "Test Role",
                roleType: RoleType.Main,
                musicalId: musical.Id
            );

            // Act 1 — создание роли
            var createdRole = await service.CreateAsync(testRole);

            // Assert 1 — проверка создания
            createdRole.Should().NotBeNull();
            createdRole.Id.Should().BeGreaterThan(0);
            createdRole.Name.Should().Be(testRole.Name);

            // Act 2 — получение роли по ID
            var retrievedRole = await service.GetByIdAsync(createdRole.Id);
            retrievedRole.Should().NotBeNull();
            retrievedRole.Name.Should().Be(testRole.Name);

            // Act 3 — обновление роли
            var updatedRole = _roleFixture.CreateRole(
                id: createdRole.Id,
                name: "Updated Role Name",
                roleType: RoleType.Supporting,
                musicalId: musical.Id
            );

            var updateResult = await service.UpdateAsync(updatedRole);
            updateResult.Should().NotBeNull();
            updateResult.Name.Should().Be("Updated Role Name");
            updateResult.RoleType.Should().Be(RoleType.Supporting);

            // Act 4 — получение ролей по мюзиклу
            var rolesByMusical = await service.GetByMusicalIdAsync(musical.Id);
            rolesByMusical.Should().Contain(r => r.Id == createdRole.Id);

            // Act 5 — получение ролей по типу
            var rolesByType = await service.GetByRoleTypeAsync(RoleType.Supporting);
            rolesByType.Should().Contain(r => r.Id == createdRole.Id);

            // Act 6 — получение всех ролей
            var allRoles = await service.GetAllAsync();
            allRoles.Should().Contain(r => r.Id == createdRole.Id);

            // Act 7 — удаление роли
            var deleteResult = await service.DeleteAsync(createdRole.Id);
            deleteResult.Should().BeTrue();

            //// Assert 7 — проверка удаления
            //var deletedRole = await service.GetByIdAsync(createdRole.Id);
            //deletedRole.Should().BeNull();
        //}
    }
}
