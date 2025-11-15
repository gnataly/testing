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
using TheatreCenter.Domain.Interfaces.Repositories;

namespace TheatreCenter.Tests.IntegrationTests.Repositories;

[CollectionDefinition("Database collection")]
[Trait("Category", TestCategories.Integration)]
public class CastMemberRepositoryIt : IntegrationTestBase
{
    private readonly CastMemberFixture _castMemberFixture = new CastMemberFixture();
    private readonly ShowFixture _showFixture = new ShowFixture();
    private readonly RoleFixture _roleFixture = new RoleFixture();
    private readonly ActorFixture _actorFixture = new ActorFixture();
    private readonly MusicalFixture _musicalFixture = new MusicalFixture();
    private readonly TheatreFixture _theatreFixture = new TheatreFixture();
    private ActorRepository _actorRepository;
    private ShowRepository _showRepository;
    private RoleRepository _roleRepository;
    private MusicalRepository _musicalRepository;
    private TheatreRepository _theatreRepository;
    private CastMemberRepository _castMemberRepository;
    private Func<Task> _commitTransaction;
    private Func<Task> _rollbackTransaction;

    public CastMemberRepositoryIt(DatabaseFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    public override async Task InitializeAsync()
    {
        //await Fixture.WaitForDatabaseReadyAsync(TimeSpan.FromSeconds(30));
        //var (repository, commit, rollback) = await Fixture.CreateTransactionalRepositoryAsync<CastMemberRepository>();
        //_repository = repository;
        //_commitTransaction = commit;
        //_rollbackTransaction = rollback;

        var context = await Fixture.CreateTransactionalContextAsync();

        _actorRepository = Fixture.CreateRepository<ActorRepository>(context);
        _showRepository = Fixture.CreateRepository<ShowRepository>(context);
        _roleRepository = Fixture.CreateRepository<RoleRepository>(context);
        _musicalRepository = Fixture.CreateRepository<MusicalRepository>(context);
        _theatreRepository = Fixture.CreateRepository<TheatreRepository>(context);
        _castMemberRepository = Fixture.CreateRepository<CastMemberRepository>(context);

        _commitTransaction = async () => { await context.Database.CommitTransactionAsync(); await context.DisposeAsync(); };
        _rollbackTransaction = async () => { await context.Database.RollbackTransactionAsync(); await context.DisposeAsync(); };
    }

    public override async Task DisposeAsync()
    {
        await _rollbackTransaction();
    }

    [Fact]
    public async Task CastMember_FullCycle_WithRelationships()
    {

        var theatre = _theatreFixture.CreateTheatre();
        await _theatreRepository.AddAsync(theatre);;

        var musical = _musicalFixture.CreateMusical(
            theatreId: theatre.Id);
        await _musicalRepository.AddAsync(musical);

        var actor = _actorFixture.CreateActor();
        await _actorRepository.AddAsync(actor);

        var role = _roleFixture.CreateRole(
            musicalId: musical.Id);
        await _roleRepository.AddAsync(role);

        var show = _showFixture.CreateShow(
            musicalId: musical.Id,
            date: DateTime.UtcNow.AddDays(7));
        await _showRepository.AddAsync(show);


        var castMember = _castMemberFixture.CreateCastMember(
            actorId: actor.Id,
            roleId: role.Id,
            showId: show.Id);

        await _castMemberRepository.AddAsync(castMember);

        var created = await _castMemberRepository.GetByIdAsync(castMember.Id);
        created.Should().NotBeNull();
        created!.ShowId.Should().Be(castMember.ShowId);
        created.RoleId.Should().Be(castMember.RoleId);
        created.ActorId.Should().Be(castMember.ActorId);
        created.Comment.Should().Be(castMember.Comment);

        created.Comment = castMember.Comment + "123";
        await _castMemberRepository.UpdateAsync(created);

        var updated = await _castMemberRepository.GetByIdAsync(castMember.Id);
        updated!.Comment.Should().Be(castMember.Comment);

        var showCast = await _castMemberRepository.GetByShowIdAsync(castMember.ShowId);

        showCast.Should().ContainSingle(cm => cm.Id == castMember.Id);

        var actorCast = await _castMemberRepository.GetByActorIdAsync(castMember.ActorId);

        actorCast.Should().ContainSingle(cm => cm.Id == castMember.Id);

        await _castMemberRepository.RemoveAsync(updated);

        var deleted = await _castMemberRepository.GetByIdAsync(castMember.Id);
        deleted.Should().BeNull();

    }
}