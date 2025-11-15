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
    private readonly TheatreFixture _theatreFixture = new TheatreFixture();
    private RoleRepository _roleRepository;
    private MusicalRepository _musicalRepository;
    private TheatreRepository _theatreRepository;
    private Func<Task> _commitTransaction;
    private Func<Task> _rollbackTransaction;

    public RoleRepositoryIt(DatabaseFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    public override async Task InitializeAsync()
    {
        var context = await Fixture.CreateTransactionalContextAsync();

        _roleRepository = Fixture.CreateRepository<RoleRepository>(context);
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
    public async Task Role_FullCycle_WithRoleType()
    {
        var theatre = _theatreFixture.CreateTheatre();
        await _theatreRepository.AddAsync(theatre);

        var musical = _musicalFixture.CreateMusical(theatreId: theatre.Id);
        await _musicalRepository.AddAsync(musical);

        var role = _roleFixture.CreateRole(
            roleType: RoleType.Main,
            musicalId: musical.Id);

        await _roleRepository.AddAsync(role);

        var created = await _roleRepository.GetByIdAsync(role.Id);
        created.Should().NotBeNull();
        created!.Name.Should().Be(role.Name);
        created.RoleType.Should().Be(RoleType.Main);
        created.MusicalId.Should().Be(role.MusicalId);

        created.Name = role.Name + "123";
        created.RoleType = RoleType.Supporting;
        await _roleRepository.UpdateAsync(created);

        var updated = await _roleRepository.GetByIdAsync(role.Id);
        updated!.Name.Should().Be(role.Name);
        updated.RoleType.Should().Be(RoleType.Supporting);

        var musicalRoles = await _roleRepository.GetByMusicalIdAsync(role.MusicalId);

        musicalRoles.Should().ContainSingle(r => r.Id == role.Id);

        var supportingRoles = await _roleRepository.GetByRoleTypeAsync(RoleType.Supporting);

        supportingRoles.Should().ContainSingle(r => r.Id == role.Id);

        await _roleRepository.RemoveAsync(updated);

        var deleted = await _roleRepository.GetByIdAsync(role.Id);
        deleted.Should().BeNull();
    }
}