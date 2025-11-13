using TheatreCenter.Data;
using TheatreCenter.Domain.Models;
using TheatreCenter.Domain.Enums;
using TheatreCenter.Services.Services;
using TheatreCenter.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Xunit.Abstractions;
using TheatreCenter.Tests.Fixtures;
using System.Diagnostics;
using TheatreCenter.UnitTests.Tests.Database;

namespace TheatreCenter.UnitTests.Tests.E2ETests;

[Collection("Database collection")]
[Trait("Category", TestCategories.E2E)]
public class TheatreServiceE2ETests : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private readonly ITestOutputHelper _output;

    // Services
    private AccountService _accountService;
    private ActorService _actorService;
    private MusicalService _musicalService;
    private TheatreService _theatreService;
    private ShowService _showService;
    private RoleService _roleService;
    private CastMemberService _castMemberService;

    // Fixtures
    private readonly AccountFixture _accountFixture = new();
    private readonly ActorFixture _actorFixture = new();
    private readonly MusicalFixture _musicalFixture = new();
    private readonly TheatreFixture _theatreFixture = new();
    private readonly ShowFixture _showFixture = new();
    private readonly RoleFixture _roleFixture = new();
    private readonly CastMemberFixture _castMemberFixture = new();

    private Func<Task> _commitTransaction;
    private Func<Task> _rollbackTransaction;

    public TheatreServiceE2ETests(DatabaseFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
        _fixture.Output = output;
    }

    public async Task InitializeAsync()
    {
        AppDbContext context = await _fixture.CreateTransactionalContextAsync();

        // Initialize repositories
        var accountRepository = new AccountRepository(context);
        var actorRepository = new ActorRepository(context, new NullLogger<ActorRepository>());
        var musicalRepository = new MusicalRepository(context);
        var theatreRepository = new TheatreRepository(context);
        var showRepository = new ShowRepository(context);
        var roleRepository = new RoleRepository(context);
        var castMemberRepository = new CastMemberRepository(context);

        // Initialize services
        _accountService = new AccountService(accountRepository);
        _actorService = new ActorService(actorRepository, new NullLogger<ActorService>());
        _musicalService = new MusicalService(musicalRepository, theatreRepository);
        _theatreService = new TheatreService(theatreRepository);
        _showService = new ShowService(showRepository, musicalRepository);
        _roleService = new RoleService(roleRepository, musicalRepository);
        _castMemberService = new CastMemberService(
            castMemberRepository,
            showRepository,
            roleRepository,
            actorRepository
        );

        _commitTransaction = async () => {
            await context.Database.CommitTransactionAsync();
            await context.DisposeAsync();
        };

        _rollbackTransaction = async () => {
            await context.Database.RollbackTransactionAsync();
            await context.DisposeAsync();
        };
    }

    public async Task DisposeAsync()
    {
        await _rollbackTransaction();
    }

    //[Fact]
    //public async Task ScenarioE2E_TheatreManagement()
    //{
    //    _output.WriteLine("=== Starting Theatre Management E2E Test ===");

    //    // Step 1: Create accounts
    //    _output.WriteLine("Step 1: Creating accounts...");
    //    var userAccount = await _accountService.RegisterAsync(
    //        $"testuser_{Guid.NewGuid():N}",
    //        "test_password_hash",
    //        AccessLevel.User
    //    );

    //    var adminAccount = await _accountService.RegisterAsync(
    //        $"testadmin_{Guid.NewGuid():N}",
    //        "test_admin_hash",
    //        AccessLevel.Admin
    //    );

    //    Assert.NotNull(userAccount);
    //    Assert.NotNull(adminAccount);
    //    _output.WriteLine($"Created user account: {userAccount.Username}, admin account: {adminAccount.Username}");

    //    // Step 2: Create theatre
    //    _output.WriteLine("Step 2: Creating theatre...");
    //    var theatre = _theatreFixture.CreateTheatre(name: "Test Theatre 001");
    //    var createdTheatre = await _theatreService.CreateTheatreAsync(theatre);

    //    Assert.NotNull(createdTheatre);
    //    Assert.Equal("Test Theatre 001", createdTheatre.Name);
    //    _output.WriteLine($"Created theatre: {createdTheatre.Name}");

    //    // Step 3: Create musical
    //    _output.WriteLine("Step 3: Creating musical...");
    //    var musical = _musicalFixture.CreateMusical(
    //        title: "Test Musical Alpha",
    //        theatreId: createdTheatre.Id
    //    );
    //    var createdMusical = await _musicalService.CreateMusicalAsync(musical);

    //    Assert.NotNull(createdMusical);
    //    Assert.Equal("Test Musical Alpha", createdMusical.Title);
    //    _output.WriteLine($"Created musical: {createdMusical.Title}");

    //    // Step 4: Create roles for the musical
    //    _output.WriteLine("Step 4: Creating roles...");
    //    var mainRole = _roleFixture.CreateRole(
    //        name: "Test Main Role 01",
    //        roleType: RoleType.Main,
    //        musicalId: createdMusical.Id
    //    );
    //    var supportingRole = _roleFixture.CreateRole(
    //        name: "Test Supporting Role 01",
    //        roleType: RoleType.Supporting,
    //        musicalId: createdMusical.Id
    //    );

    //    var createdMainRole = await _roleService.CreateAsync(mainRole);
    //    var createdSupportingRole = await _roleService.CreateAsync(supportingRole);

    //    Assert.NotNull(createdMainRole);
    //    Assert.NotNull(createdSupportingRole);
    //    _output.WriteLine($"Created roles: {createdMainRole.Name}, {createdSupportingRole.Name}");

    //    // Step 5: Create actors
    //    _output.WriteLine("Step 5: Creating actors...");
    //    var mainActor = _actorFixture.CreateActor(
    //        name: "Test Actor Main",
    //        voiceType: VoiceType.Tenor,
    //        gender: Gender.Male
    //    );
    //    var supportingActor = _actorFixture.CreateActor(
    //        name: "Test Actor Supporting",
    //        voiceType: VoiceType.Baritone,
    //        gender: Gender.Male
    //    );

    //    var createdMainActor = await _actorService.CreateActorAsync(mainActor);
    //    var createdSupportingActor = await _actorService.CreateActorAsync(supportingActor);

    //    Assert.NotNull(createdMainActor);
    //    Assert.NotNull(createdSupportingActor);
    //    _output.WriteLine($"Created actors: {createdMainActor.Name}, {createdSupportingActor.Name}");

    //    // Step 6: Create show
    //    _output.WriteLine("Step 6: Creating show...");
    //    var show = _showFixture.CreateShow(
    //        musicalId: createdMusical.Id,
    //        date: DateTime.UtcNow.AddDays(30)
    //    );
    //    var createdShow = await _showService.CreateAsync(show);

    //    Assert.NotNull(createdShow);
    //    _output.WriteLine($"Created show for date: {createdShow.Date}");

    //    // Step 7: Assign cast members
    //    _output.WriteLine("Step 7: Assigning cast members...");
    //    var mainCastMember = _castMemberFixture.CreateCastMember(
    //        showId: createdShow.Id,
    //        roleId: createdMainRole.Id,
    //        actorId: createdMainActor.Id,
    //        comment: "Test main role assignment"
    //    );
    //    var supportingCastMember = _castMemberFixture.CreateCastMember(
    //        showId: createdShow.Id,
    //        roleId: createdSupportingRole.Id,
    //        actorId: createdSupportingActor.Id,
    //        comment: "Test supporting role assignment"
    //    );

    //    var createdMainCast = await _castMemberService.CreateAsync(mainCastMember);
    //    var createdSupportingCast = await _castMemberService.CreateAsync(supportingCastMember);

    //    Assert.NotNull(createdMainCast);
    //    Assert.NotNull(createdSupportingCast);
    //    _output.WriteLine($"Assigned cast members for the show");

    //    // Step 8: Verify data retrieval
    //    _output.WriteLine("Step 8: Verifying data retrieval...");

    //    // Get show cast
    //    var showCast = await _castMemberService.GetByShowIdAsync(createdShow.Id);
    //    Assert.Equal(2, showCast.Count());
    //    _output.WriteLine($"Show has {showCast.Count()} cast members");

    //    // Get actor's roles
    //    var mainActorRoles = await _castMemberService.GetByActorIdAsync(createdMainActor.Id);
    //    Assert.Single(mainActorRoles);
    //    _output.WriteLine($"Actor {createdMainActor.Name} has {mainActorRoles.Count()} role(s)");

    //    // Get musicals by theatre
    //    var theatreMusicals = await _musicalService.GetMusicalsByTheatreAsync(createdTheatre.Id);
    //    Assert.Single(theatreMusicals);
    //    _output.WriteLine($"Theatre has {theatreMusicals.Count()} musical(s)");

    //    // Get roles by musical
    //    var musicalRoles = await _roleService.GetByMusicalIdAsync(createdMusical.Id);
    //    Assert.Equal(2, musicalRoles.Count());
    //    _output.WriteLine($"Musical has {musicalRoles.Count()} roles");

    //    // Step 9: Test updates
    //    _output.WriteLine("Step 9: Testing updates...");

    //    // Update actor
    //    var updatedActor = _actorFixture.CreateActor(
    //        id: createdMainActor.Id,
    //        name: "Test Actor Main Updated",
    //        voiceType: VoiceType.Tenor,
    //        gender: Gender.Male
    //    );
    //    var resultActor = await _actorService.UpdateActorAsync(updatedActor);
    //    Assert.Equal("Test Actor Main Updated", resultActor.Name);
    //    _output.WriteLine($"Updated actor name to: {resultActor.Name}");

    //    // Update musical
    //    var updatedMusical = _musicalFixture.CreateMusical(
    //        id: createdMusical.Id,
    //        title: "Test Musical Alpha Updated",
    //        theatreId: createdTheatre.Id
    //    );
    //    var resultMusical = await _musicalService.UpdateMusicalAsync(updatedMusical);
    //    Assert.Equal("Test Musical Alpha Updated", resultMusical.Title);
    //    _output.WriteLine($"Updated musical title to: {resultMusical.Title}");

    //    // Step 10: Cleanup verification
    //    _output.WriteLine("Step 10: Testing cleanup operations...");

    //    // Delete cast members
    //    await _castMemberService.DeleteAsync(createdMainCast.Id);
    //    await _castMemberService.DeleteAsync(createdSupportingCast.Id);

    //    // Delete show
    //    await _showService.DeleteAsync(createdShow.Id);

    //    // Verify deletions
    //    var remainingCast = await _castMemberService.GetByShowIdAsync(createdShow.Id);
    //    Assert.Empty(remainingCast);
    //    _output.WriteLine("Cleanup operations completed successfully");

    //    _output.WriteLine("=== Theatre Management E2E Test Completed Successfully ===");
    //}

    //[Fact]
    //public async Task ScenarioE2E_UserAuthentication()
    //{
    //    _output.WriteLine("=== Starting User Authentication E2E Test ===");

    //    // Step 1: User registration
    //    _output.WriteLine("Step 1: User registration...");
    //    var username = $"testuser_{Guid.NewGuid():N}";
    //    var passwordHash = "test_hashed_password_123";

    //    var registeredAccount = await _accountService.RegisterAsync(username, passwordHash, AccessLevel.User);
    //    Assert.NotNull(registeredAccount);
    //    Assert.Equal(username, registeredAccount.Username);
    //    _output.WriteLine($"Registered user: {username}");

    //    // Step 2: User authentication
    //    _output.WriteLine("Step 2: User authentication...");
    //    var authenticatedAccount = await _accountService.AuthenticateAsync(username, passwordHash);
    //    Assert.NotNull(authenticatedAccount);
    //    Assert.Equal(registeredAccount.Id, authenticatedAccount.Id);
    //    _output.WriteLine("User authenticated successfully");

    //    // Step 3: Create test data
    //    _output.WriteLine("Step 3: Creating test data...");

    //    var theatre = _theatreFixture.CreateTheatre(name: "Test Theatre Auth");
    //    var createdTheatre = await _theatreService.CreateTheatreAsync(theatre);

    //    var musical = _musicalFixture.CreateMusical(
    //        title: "Test Musical Beta",
    //        theatreId: createdTheatre.Id
    //    );
    //    var createdMusical = await _musicalService.CreateMusicalAsync(musical);

    //    var actor = _actorFixture.CreateActor(name: "Test Actor Auth");
    //    var createdActor = await _actorService.CreateActorAsync(actor);

    //    _output.WriteLine("Test data created successfully");

    //    // Step 4: Password change
    //    _output.WriteLine("Step 4: Changing password...");
    //    var newPasswordHash = "test_new_hashed_password_456";
    //    await _accountService.ChangePasswordAsync(registeredAccount.Id, newPasswordHash);

    //    // Verify new password works
    //    var reauthenticatedAccount = await _accountService.AuthenticateAsync(username, newPasswordHash);
    //    Assert.NotNull(reauthenticatedAccount);
    //    _output.WriteLine("Password changed successfully");

    //    // Step 5: Account deletion
    //    _output.WriteLine("Step 5: Account deletion...");
    //    await _accountService.DeleteAsync(registeredAccount.Id);

    //    // Verify account is deleted
    //    var deletedAccount = await _accountService.GetByIdAsync(registeredAccount.Id);
    //    Assert.Null(deletedAccount);
    //    _output.WriteLine("Account deleted successfully");

    //    _output.WriteLine("=== User Authentication E2E Test Completed Successfully ===");
    //}

    [Fact]
    public async Task ScenarioE2E_MusicalAndRolesManagement()
    {
        _output.WriteLine("=== Starting Musical and Roles Management E2E Test ===");

        // Step 1: Create theatre and musical
        _output.WriteLine("Step 1: Creating theatre and musical...");

        var theatre = _theatreFixture.CreateTheatre(name: "Test Theatre Roles");
        var createdTheatre = await _theatreService.CreateTheatreAsync(theatre);

        var musical = _musicalFixture.CreateMusical(
            title: "Test Musical Gamma",
            theatreId: createdTheatre.Id
        );
        var createdMusical = await _musicalService.CreateMusicalAsync(musical);

        _output.WriteLine($"Created theatre: {createdTheatre.Name}, musical: {createdMusical.Title}");

        // Step 2: Create multiple roles
        _output.WriteLine("Step 2: Creating multiple roles...");

        var role1 = _roleFixture.CreateRole(
            name: "Test Role Alpha",
            roleType: RoleType.Main,
            musicalId: createdMusical.Id
        );
        var role2 = _roleFixture.CreateRole(
            name: "Test Role Beta",
            roleType: RoleType.Supporting,
            musicalId: createdMusical.Id
        );
        var role3 = _roleFixture.CreateRole(
            name: "Test Role Gamma",
            roleType: RoleType.Ensemble,
            musicalId: createdMusical.Id
        );

        var createdRole1 = await _roleService.CreateAsync(role1);
        var createdRole2 = await _roleService.CreateAsync(role2);
        var createdRole3 = await _roleService.CreateAsync(role3);

        Assert.NotNull(createdRole1);
        Assert.NotNull(createdRole2);
        Assert.NotNull(createdRole3);
        _output.WriteLine($"Created 3 roles: {createdRole1.Name}, {createdRole2.Name}, {createdRole3.Name}");

        // Step 3: Get roles by type
        _output.WriteLine("Step 3: Getting roles by type...");

        var mainRoles = await _roleService.GetByRoleTypeAsync(RoleType.Main);
        var supportingRoles = await _roleService.GetByRoleTypeAsync(RoleType.Supporting);
        var ensembleRoles = await _roleService.GetByRoleTypeAsync(RoleType.Ensemble);

        Assert.Contains(mainRoles, r => r.Name == "Test Role Alpha");
        Assert.Contains(supportingRoles, r => r.Name == "Test Role Beta");
        Assert.Contains(ensembleRoles, r => r.Name == "Test Role Gamma");
        _output.WriteLine("Roles filtered by type successfully");

        // Step 4: Get roles by musical
        _output.WriteLine("Step 4: Getting roles by musical...");

        var musicalRoles = await _roleService.GetByMusicalIdAsync(createdMusical.Id);
        Assert.Equal(3, musicalRoles.Count());
        _output.WriteLine($"Musical has {musicalRoles.Count()} roles");

        // Step 5: Update role
        _output.WriteLine("Step 5: Updating role...");

        var updatedRole = _roleFixture.CreateRole(
            id: createdRole1.Id,
            name: "Test Role Alpha Updated",
            roleType: RoleType.Main,
            musicalId: createdMusical.Id
        );
        var resultRole = await _roleService.UpdateAsync(updatedRole);
        Assert.Equal("Test Role Alpha Updated", resultRole.Name);
        _output.WriteLine($"Updated role name to: {resultRole.Name}");

        // Step 6: Delete role
        _output.WriteLine("Step 6: Deleting role...");

        var deleteResult = await _roleService.DeleteAsync(createdRole3.Id);
        Assert.True(deleteResult);

        var remainingRoles = await _roleService.GetByMusicalIdAsync(createdMusical.Id);
        Assert.Equal(2, remainingRoles.Count());
        _output.WriteLine("Role deleted successfully");

        _output.WriteLine("=== Musical and Roles Management E2E Test Completed Successfully ===");
    }
}

public class TestDbContextFactory : IDbContextFactory<AppDbContext>
{
    private readonly AppDbContext _context;

    public TestDbContextFactory(AppDbContext context)
    {
        _context = context;
    }

    public AppDbContext CreateDbContext()
    {
        return _context;
    }
}