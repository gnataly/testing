using Allure.Xunit.Attributes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TheatreCenter.Data;
using TheatreCenter.Data.Repositories;
using TheatreCenter.Domain.Enums;
using TheatreCenter.Domain.Models;
using TheatreCenter.Tests.Fixtures;
using TheatreCenter.UnitTests;
using Xunit;

namespace TheatreCenter.Tests.Repositories;

[AllureSuite("Role Repository Tests")]
[Trait("Category", TestCategories.Unit)]
public class RoleRepositoryTests : IClassFixture<RoleFixture>
{
    private readonly RoleFixture _fixture;

    public RoleRepositoryTests(RoleFixture fixture)
    {
        _fixture = fixture;
    }

    private RoleRepository GetInMemoryRepository()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);
        return new RoleRepository(context);
    }

    [Fact]
    [AllureFeature("GetByIdAsync")]
    [AllureStory("Positive case - role exists")]
    public async Task GetByIdAsync_RoleExists_ReturnsRole()
    {
        var contextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new AppDbContext(contextOptions);

        var theatreRepository = new TheatreRepository(context);
        var musicalRepository = new MusicalRepository(context);
        var roleRepository = new RoleRepository(context);

        var theatre = new Theatre(1, "Test Theatre", "Test Info");
        await theatreRepository.AddAsync(theatre);
        await theatreRepository.SaveChangesAsync();

        var musical = new Musical(1, "Test Musical", "Test Description", TimeSpan.FromHours(2),
                      AgeRestriction.EighteenPlus, 1);
        await musicalRepository.AddAsync(musical);
        await musicalRepository.SaveChangesAsync();

        var role = _fixture.CreateRole(musicalId: 1);

        await roleRepository.AddAsync(role);
        await roleRepository.SaveChangesAsync();



        var result = await roleRepository.GetByIdAsync(role.Id);


        result.Should().NotBeNull();
        result.Id.Should().Be(role.Id);
        result.Name.Should().Be(role.Name);
    }

    [Fact]
    [AllureFeature("GetByIdAsync")]
    [AllureStory("Negative case - role not found")]
    public async Task GetByIdAsync_RoleNotExists_ReturnsNull()
    {

        var repository = GetInMemoryRepository();


        var result = await repository.GetByIdAsync(999);


        result.Should().BeNull();
    }

    [Fact]
    [AllureFeature("GetAllAsync")]
    [AllureStory("Positive case - returns all roles")]
    public async Task GetAllAsync_RolesExist_ReturnsRoles()
    {
        var contextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new AppDbContext(contextOptions);

        var theatreRepository = new TheatreRepository(context);
        var musicalRepository = new MusicalRepository(context);
        var roleRepository = new RoleRepository(context);

        var theatre = new Theatre(1, "Test Theatre", "Test Info");
        await theatreRepository.AddAsync(theatre);
        await theatreRepository.SaveChangesAsync();

        var musical = new Musical(1, "Test Musical", "Test Description", TimeSpan.FromHours(2),
                      AgeRestriction.EighteenPlus, 1);
        await musicalRepository.AddAsync(musical);
        await musicalRepository.SaveChangesAsync();

        var role1 = _fixture.CreateRole(name: "Role 1", musicalId: 1);
        var role2 = _fixture.CreateRole(name: "Role 2", musicalId: 1);

        await roleRepository.AddAsync(role1);
        await roleRepository.AddAsync(role2);
        await roleRepository.SaveChangesAsync();



        var result = await roleRepository.GetAllAsync(new RoleFilter());


        result.Should().HaveCount(2);
        result.Should().Contain(r => r.Name == "Role 1");
        result.Should().Contain(r => r.Name == "Role 2");
    }

    [Fact]
    [AllureFeature("GetAllAsync")]
    [AllureStory("Negative case - no roles")]
    public async Task GetAllAsync_NoRoles_ReturnsEmpty()
    {

        var repository = GetInMemoryRepository();


        var result = await repository.GetAllAsync(new RoleFilter());


        result.Should().BeEmpty();
    }

    [Fact]
    [AllureFeature("AddAsync")]
    [AllureStory("Positive case - adds role")]
    public async Task AddAsync_ValidRole_AddsToDatabase()
    {
        var contextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new AppDbContext(contextOptions);

        var theatreRepository = new TheatreRepository(context);
        var musicalRepository = new MusicalRepository(context);
        var roleRepository = new RoleRepository(context);

        var theatre = new Theatre(1, "Test Theatre", "Test Info");
        await theatreRepository.AddAsync(theatre);
        await theatreRepository.SaveChangesAsync();

        var musical = new Musical(1, "Test Musical", "Test Description", TimeSpan.FromHours(2),
                      AgeRestriction.EighteenPlus, 1);
        await musicalRepository.AddAsync(musical);
        await musicalRepository.SaveChangesAsync();

        var role = _fixture.CreateRole(musicalId: 1);

        await roleRepository.AddAsync(role);
        await roleRepository.SaveChangesAsync();



        var result = await roleRepository.GetByIdAsync(role.Id);
        result.Should().NotBeNull();
    }

    [Fact]
    [AllureFeature("UpdateAsync")]
    [AllureStory("Positive case - updates role")]
    public async Task UpdateAsync_ValidRole_UpdatesSuccessfully()
    {
        var contextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new AppDbContext(contextOptions);

        var theatreRepository = new TheatreRepository(context);
        var musicalRepository = new MusicalRepository(context);
        var roleRepository = new RoleRepository(context);

        var theatre = new Theatre(1, "Test Theatre", "Test Info");
        await theatreRepository.AddAsync(theatre);
        await theatreRepository.SaveChangesAsync();

        var musical = new Musical(1, "Test Musical", "Test Description", TimeSpan.FromHours(2),
                      AgeRestriction.EighteenPlus, 1);
        await musicalRepository.AddAsync(musical);
        await musicalRepository.SaveChangesAsync();

        var role = _fixture.CreateRole(name: "Original Name", musicalId: 1);

        await roleRepository.AddAsync(role);
        await roleRepository.SaveChangesAsync();


        var existingRole = await roleRepository.GetByIdAsync(role.Id);
        existingRole.Name = "Updated Name";


        await roleRepository.UpdateAsync(existingRole);
        await roleRepository.SaveChangesAsync();


        var result = await roleRepository.GetByIdAsync(role.Id);
        result.Name.Should().Be("Updated Name");
    }

    [Fact]
    [AllureFeature("RemoveAsync")]
    [AllureStory("Positive case - removes role")]
    public async Task RemoveAsync_RoleExists_RemovesRole()
    {

        var repository = GetInMemoryRepository();
        var role = _fixture.CreateRole();

        await repository.AddAsync(role);
        await repository.SaveChangesAsync();



        await repository.RemoveAsync(role);
        await repository.SaveChangesAsync();



        var result = await repository.GetByIdAsync(role.Id);
        result.Should().BeNull();
    }

    [Fact]
    [AllureFeature("GetByMusicalIdAsync")]
    [AllureStory("Positive case - returns roles by musical")]
    public async Task GetByMusicalIdAsync_ValidMusicalId_ReturnsRoles()
    {
        var contextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new AppDbContext(contextOptions);

        var theatreRepository = new TheatreRepository(context);
        var musicalRepository = new MusicalRepository(context);
        var roleRepository = new RoleRepository(context);

        var theatre1 = new Theatre(1, "Test Theatre 1", "Test Info");
        var theatre2 = new Theatre(2, "Test Theatre 2", "Test Info");
        await theatreRepository.AddAsync(theatre1);
        await theatreRepository.AddAsync(theatre2);
        await theatreRepository.SaveChangesAsync();

        var musical1 = new Musical(1, "Test Musical 1", "Test Description", TimeSpan.FromHours(2),
                      AgeRestriction.EighteenPlus, 1);
        var musical2 = new Musical(2, "Test Musical 2", "Test Description", TimeSpan.FromHours(2),
                      AgeRestriction.EighteenPlus, 2);
        await musicalRepository.AddAsync(musical1);
        await musicalRepository.AddAsync(musical2);
        await musicalRepository.SaveChangesAsync();

        var musicalId = 1;
        var role1 = _fixture.CreateRole(musicalId: musicalId, name: "Role 1");
        var role2 = _fixture.CreateRole(musicalId: musicalId, name: "Role 2");
        var role3 = _fixture.CreateRole(musicalId: 2, name: "Role 3");

        await roleRepository.AddAsync(role1);
        await roleRepository.AddAsync(role2);
        await roleRepository.AddAsync(role3);
        await roleRepository.SaveChangesAsync();



        var result = await roleRepository.GetByMusicalIdAsync(musicalId);


        result.Should().HaveCount(2);
        result.Should().Contain(r => r.Name == "Role 1");
        result.Should().Contain(r => r.Name == "Role 2");
    }

    [Fact]
    [AllureFeature("GetByMusicalIdAsync")]
    [AllureStory("Negative case - no roles for musical")]
    public async Task GetByMusicalIdAsync_NoRolesForMusical_ReturnsEmpty()
    {

        var repository = GetInMemoryRepository();
        var role = _fixture.CreateRole(musicalId: 1);

        await repository.AddAsync(role);
        await repository.SaveChangesAsync();



        var result = await repository.GetByMusicalIdAsync(999);


        result.Should().BeEmpty();
    }

    [Fact]
    [AllureFeature("GetByRoleTypeAsync")]
    [AllureStory("Positive case - returns roles by type")]
    public async Task GetByRoleTypeAsync_ValidRoleType_ReturnsRoles()
    {
        var contextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new AppDbContext(contextOptions);

        var theatreRepository = new TheatreRepository(context);
        var musicalRepository = new MusicalRepository(context);
        var roleRepository = new RoleRepository(context);

        var theatre = new Theatre(1, "Test Theatre", "Test Info");
        await theatreRepository.AddAsync(theatre);
        await theatreRepository.SaveChangesAsync();

        var musical = new Musical(1, "Test Musical", "Test Description", TimeSpan.FromHours(2),
                      AgeRestriction.EighteenPlus, 1);
        await musicalRepository.AddAsync(musical);
        await musicalRepository.SaveChangesAsync();

        var mainRole1 = _fixture.CreateRole(roleType: RoleType.Main, name: "Main Role 1", musicalId: 1);
        var mainRole2 = _fixture.CreateRole(roleType: RoleType.Main, name: "Main Role 2", musicalId: 1);
        var supportingRole = _fixture.CreateRole(roleType: RoleType.Supporting, name: "Supporting Role", musicalId: 1);

        await roleRepository.AddAsync(mainRole1);
        await roleRepository.AddAsync(mainRole2);
        await roleRepository.AddAsync(supportingRole);
        await roleRepository.SaveChangesAsync();



        var result = await roleRepository.GetByRoleTypeAsync(RoleType.Main);


        result.Should().HaveCount(2);
        result.Should().Contain(r => r.Name == "Main Role 1");
        result.Should().Contain(r => r.Name == "Main Role 2");
    }

    [Fact]
    [AllureFeature("GetByRoleTypeAsync")]
    [AllureStory("Negative case - no roles with type")]
    public async Task GetByRoleTypeAsync_NoRolesWithType_ReturnsEmpty()
    {

        var repository = GetInMemoryRepository();
        var role = _fixture.CreateRole(roleType: RoleType.Main);

        await repository.AddAsync(role);
        await repository.SaveChangesAsync();



        var result = await repository.GetByRoleTypeAsync(RoleType.Supporting);


        result.Should().BeEmpty();
    }
}
