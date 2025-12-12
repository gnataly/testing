using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TheatreCenter.Data.Repositories;
using TheatreCenter.Data;
using TheatreCenter.Domain.Enums;
using TheatreCenter.Domain.Models;
using TheatreCenter.Services.Services;
using TheatreCenter.Tests.Fixtures;
using TheatreCenter.UnitTests.Tests.Database;
using Xunit;
using FluentAssertions;
using Xunit.Abstractions;
using TheatreCenter.UnitTests;

namespace TheatreCenter.UnitTests.Tests.IntegrationTests.Services;

[CollectionDefinition("Database collection")]
[Trait("Category", TestCategories.Integration)]
public class RoleServiceIt : IntegrationTestBase
{
    private readonly RoleFixture _roleFixture = new RoleFixture();
    private readonly MusicalFixture _musicalFixture = new MusicalFixture();
    private readonly TheatreFixture _theatreFixture = new TheatreFixture();
    private MusicalRepository _musicalRepository;
    private TheatreRepository _theatreRepository;
    private RoleService _service;
    private Func<Task> _commitTransaction;
    private Func<Task> _rollbackTransaction;

    public RoleServiceIt(DatabaseFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    public override async Task InitializeAsync()
    {
        //await Fixture.WaitForDatabaseReadyAsync(TimeSpan.FromSeconds(30));
        var context = await Fixture.CreateTransactionalContextAsync();
        var roleRepository = Fixture.CreateRepository<RoleRepository>(context);
        _musicalRepository = Fixture.CreateRepository<MusicalRepository>(context);
        _theatreRepository = Fixture.CreateRepository<TheatreRepository>(context);
        _service = new RoleService(roleRepository, _musicalRepository, new NullLogger<RoleService>());

        _commitTransaction = async () =>
        {
            await context.Database.CommitTransactionAsync();
            await context.DisposeAsync();
        };
        _rollbackTransaction = async () =>
        {
            await context.Database.RollbackTransactionAsync();
            await context.DisposeAsync();
        };
    }

    public override async Task DisposeAsync()
    {
        await _rollbackTransaction();
    }

    [Fact]
    public async Task Role_FullCycle_WithFixtures()
    {

        var theatre = _theatreFixture.CreateTheatre();
        await _theatreRepository.AddAsync(theatre);
        await _theatreRepository.SaveChangesAsync();

        var musical = _musicalFixture.CreateMusical(
            theatreId: theatre.Id);
        await _musicalRepository.AddAsync(musical);
        await _musicalRepository.SaveChangesAsync();


        var testRole = _roleFixture.CreateRole(
            roleType: RoleType.Main,
            musicalId: musical.Id
        );

        var createdRole = await _service.CreateAsync(testRole);

        createdRole.Should().NotBeNull();
        createdRole.Id.Should().BeGreaterThan(0);
        createdRole.Name.Should().Be(testRole.Name);

        var retrievedRole = await _service.GetByIdAsync(createdRole.Id);
        retrievedRole.Should().NotBeNull();
        retrievedRole.Name.Should().Be(testRole.Name);

        var updatedRole = _roleFixture.CreateRole(
            id: createdRole.Id,
            roleType: RoleType.Supporting,
            musicalId: testRole.MusicalId
        );

        var updateResult = await _service.UpdateAsync(updatedRole);
        updateResult.Should().NotBeNull();
        updateResult.Name.Should().Be(updatedRole.Name);
        updateResult.RoleType.Should().Be(RoleType.Supporting);

        var rolesByMusical = await _service.GetByMusicalIdAsync(testRole.MusicalId);
        rolesByMusical.Should().Contain(r => r.Id == createdRole.Id);

        var rolesByType = await _service.GetByRoleTypeAsync(RoleType.Supporting);
        rolesByType.Should().Contain(r => r.Id == createdRole.Id);

        var allRoles = await _service.GetAllAsync(new RoleFilter());
        allRoles.Should().Contain(r => r.Id == createdRole.Id);

        var deleteResult = await _service.DeleteAsync(createdRole.Id);
        deleteResult.Should().BeTrue();
    }
}
