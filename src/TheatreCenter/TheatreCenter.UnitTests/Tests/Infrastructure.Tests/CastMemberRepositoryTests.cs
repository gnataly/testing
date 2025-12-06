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

[AllureSuite("Cast Member Repository Tests")]
[Trait("Category", TestCategories.Unit)]
public class CastMemberRepositoryTests : IClassFixture<CastMemberFixture>
{
    private readonly CastMemberFixture _fixture;

    public CastMemberRepositoryTests(CastMemberFixture fixture)
    {
        _fixture = fixture;
    }

    private CastMemberRepository GetInMemoryRepository()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);
        return new CastMemberRepository(context);
    }

    [Fact]
    [AllureFeature("GetByIdAsync")]
    [AllureStory("Positive case - cast member exists")]
    public async Task GetByIdAsync_CastMemberExists_ReturnsCastMember()
    {
        var contextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new AppDbContext(contextOptions);

        var theatreRepository = new TheatreRepository(context);
        var musicalRepository = new MusicalRepository(context);
        var showRepository = new ShowRepository(context);
        var actorRepository = new ActorRepository(context);
        var roleRepository = new RoleRepository(context);
        var castMemberRepository = new CastMemberRepository(context);

        var theatre = new Theatre(1, "Test Theatre", "Test Info");
        await theatreRepository.AddAsync(theatre);
        await theatreRepository.SaveChangesAsync();

        var musical = new Musical(1, "Test Musical", "Test Description", TimeSpan.FromHours(2),
                      AgeRestriction.EighteenPlus, 1);
        await musicalRepository.AddAsync(musical);
        await musicalRepository.SaveChangesAsync();

        var show = new Show(1, DateTime.UtcNow.AddDays(7), 1);
        await showRepository.AddAsync(show);
        await showRepository.SaveChangesAsync();

        var actor = new Actor(1, "Test Actor", VoiceType.Tenor, Gender.Male, DateTime.Now.AddYears(-30), 175, 70, "Bio");
        await actorRepository.AddAsync(actor);
        await actorRepository.SaveChangesAsync();

        var role = new Role(1, "Test Role", 1, RoleType.Main);
        await roleRepository.AddAsync(role);
        await roleRepository.SaveChangesAsync();

        var castMember = _fixture.CreateCastMember(showId: 1, roleId: 1, actorId: 1);

        await castMemberRepository.AddAsync(castMember);
        await castMemberRepository.SaveChangesAsync();



        var result = await castMemberRepository.GetByIdAsync(castMember.Id);


        result.Should().NotBeNull();
        result.Id.Should().Be(castMember.Id);
        result.ShowId.Should().Be(castMember.ShowId);
    }

    [Fact]
    [AllureFeature("GetByIdAsync")]
    [AllureStory("Negative case - cast member not found")]
    public async Task GetByIdAsync_CastMemberNotExists_ReturnsNull()
    {

        var repository = GetInMemoryRepository();


        var result = await repository.GetByIdAsync(999);


        result.Should().BeNull();
    }

    [Fact]
    [AllureFeature("GetAllAsync")]
    [AllureStory("Positive case - returns all cast members")]
    public async Task GetAllAsync_CastMembersExist_ReturnsCastMembers()
    {
        var contextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new AppDbContext(contextOptions);

        var theatreRepository = new TheatreRepository(context);
        var musicalRepository = new MusicalRepository(context);
        var showRepository = new ShowRepository(context);
        var actorRepository = new ActorRepository(context);
        var roleRepository = new RoleRepository(context);
        var castMemberRepository = new CastMemberRepository(context);

        var theatre = new Theatre(1, "Test Theatre", "Test Info");
        await theatreRepository.AddAsync(theatre);
        await theatreRepository.SaveChangesAsync();

        var musical = new Musical(1, "Test Musical", "Test Description", TimeSpan.FromHours(2),
                      AgeRestriction.EighteenPlus, 1);
        await musicalRepository.AddAsync(musical);
        await musicalRepository.SaveChangesAsync();

        var show1 = new Show(1, DateTime.UtcNow.AddDays(7), 1);
        var show2 = new Show(2, DateTime.UtcNow.AddDays(8), 1);
        await showRepository.AddAsync(show1);
        await showRepository.AddAsync(show2);
        await showRepository.SaveChangesAsync();

        var actor1 = new Actor(1, "Test Actor", VoiceType.Tenor, Gender.Male, DateTime.Now.AddYears(-30), 175, 70, "Bio");
        var actor2 = new Actor(2, "Test Actor 2", VoiceType.Bass, Gender.Female, DateTime.Now.AddYears(-35), 165, 65, "Bio");
        await actorRepository.AddAsync(actor1);
        await actorRepository.AddAsync(actor2);
        await actorRepository.SaveChangesAsync();

        var role1 = new Role(1, "Test Role", 1, RoleType.Main);
        var role2 = new Role(2, "Test Role 2", 1, RoleType.Supporting);
        await roleRepository.AddAsync(role1);
        await roleRepository.AddAsync(role2);
        await roleRepository.SaveChangesAsync();

        var castMember1 = _fixture.CreateCastMember(showId: 1, actorId: 1, roleId: 1);
        var castMember2 = _fixture.CreateCastMember(showId: 2, actorId: 2, roleId: 2);

        await castMemberRepository.AddAsync(castMember1);
        await castMemberRepository.AddAsync(castMember2);
        await castMemberRepository.SaveChangesAsync();



        var result = await castMemberRepository.GetAllAsync(new CastMemberFilter());


        result.Should().HaveCount(2);
    }

    [Fact]
    [AllureFeature("GetAllAsync")]
    [AllureStory("Negative case - no cast members")]
    public async Task GetAllAsync_NoCastMembers_ReturnsEmpty()
    {

        var repository = GetInMemoryRepository();


        var result = await repository.GetAllAsync(new CastMemberFilter());


        result.Should().BeEmpty();
    }

    [Fact]
    [AllureFeature("AddAsync")]
    [AllureStory("Positive case - adds cast member")]
    public async Task AddAsync_ValidCastMember_AddsToDatabase()
    {
        var contextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new AppDbContext(contextOptions);

        var theatreRepository = new TheatreRepository(context);
        var musicalRepository = new MusicalRepository(context);
        var showRepository = new ShowRepository(context);
        var actorRepository = new ActorRepository(context);
        var roleRepository = new RoleRepository(context);
        var castMemberRepository = new CastMemberRepository(context);

        var theatre = new Theatre(1, "Test Theatre", "Test Info");
        await theatreRepository.AddAsync(theatre);
        await theatreRepository.SaveChangesAsync();

        var musical = new Musical(1, "Test Musical", "Test Description", TimeSpan.FromHours(2),
                      AgeRestriction.EighteenPlus, 1);
        await musicalRepository.AddAsync(musical);
        await musicalRepository.SaveChangesAsync();

        var show = new Show(1, DateTime.UtcNow.AddDays(7), 1);
        await showRepository.AddAsync(show);
        await showRepository.SaveChangesAsync();

        var actor = new Actor(1, "Test Actor", VoiceType.Tenor, Gender.Male, DateTime.Now.AddYears(-30), 175, 70, "Bio");
        await actorRepository.AddAsync(actor);
        await actorRepository.SaveChangesAsync();

        var role = new Role(1, "Test Role", 1, RoleType.Main);
        await roleRepository.AddAsync(role);
        await roleRepository.SaveChangesAsync();

        var castMember = _fixture.CreateCastMember(showId: 1, actorId: 1, roleId: 1);


        await castMemberRepository.AddAsync(castMember);
        await castMemberRepository.SaveChangesAsync();



        var result = await castMemberRepository.GetByIdAsync(castMember.Id);
        result.Should().NotBeNull();
    }

    [Fact]
    [AllureFeature("UpdateAsync")]
    [AllureStory("Positive case - updates cast member")]
    public async Task UpdateAsync_ValidCastMember_UpdatesSuccessfully()
    {
        var contextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new AppDbContext(contextOptions);

        var theatreRepository = new TheatreRepository(context);
        var musicalRepository = new MusicalRepository(context);
        var showRepository = new ShowRepository(context);
        var actorRepository = new ActorRepository(context);
        var roleRepository = new RoleRepository(context);
        var castMemberRepository = new CastMemberRepository(context);

        var theatre = new Theatre(1, "Test Theatre", "Test Info");
        await theatreRepository.AddAsync(theatre);
        await theatreRepository.SaveChangesAsync();

        var musical = new Musical(1, "Test Musical", "Test Description", TimeSpan.FromHours(2),
                      AgeRestriction.EighteenPlus, 1);
        await musicalRepository.AddAsync(musical);
        await musicalRepository.SaveChangesAsync();

        var show = new Show(1, DateTime.UtcNow.AddDays(7), 1);
        await showRepository.AddAsync(show);
        await showRepository.SaveChangesAsync();

        var actor = new Actor(1, "Test Actor", VoiceType.Tenor, Gender.Male, DateTime.Now.AddYears(-30), 175, 70, "Bio");
        await actorRepository.AddAsync(actor);
        await actorRepository.SaveChangesAsync();

        var role = new Role(1, "Test Role", 1, RoleType.Main);
        await roleRepository.AddAsync(role);
        await roleRepository.SaveChangesAsync();

        var castMember = _fixture.CreateCastMember(comment: "Original comment", showId: 1, actorId: 1, roleId: 1);

        await castMemberRepository.AddAsync(castMember);
        await castMemberRepository.SaveChangesAsync();


        var existingCastMember = await castMemberRepository.GetByIdAsync(castMember.Id);
        existingCastMember.Comment = "Updated comment";


        await castMemberRepository.UpdateAsync(existingCastMember);
        await castMemberRepository.SaveChangesAsync();


        var result = await castMemberRepository.GetByIdAsync(castMember.Id);
        result.Comment.Should().Be("Updated comment");
    }

    [Fact]
    [AllureFeature("RemoveAsync")]
    [AllureStory("Positive case - removes cast member")]
    public async Task RemoveAsync_CastMemberExists_RemovesCastMember()
    {

        var repository = GetInMemoryRepository();
        var castMember = _fixture.CreateCastMember();

        await repository.AddAsync(castMember);
        await repository.SaveChangesAsync();



        await repository.RemoveAsync(castMember);
        await repository.SaveChangesAsync();



        var result = await repository.GetByIdAsync(castMember.Id);
        result.Should().BeNull();
    }

    [Fact]
    [AllureFeature("GetByShowIdAsync")]
    [AllureStory("Positive case - returns cast members by show")]
    public async Task GetByShowIdAsync_ValidShowId_ReturnsCastMembers()
    {
        var contextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new AppDbContext(contextOptions);

        var theatreRepository = new TheatreRepository(context);
        var musicalRepository = new MusicalRepository(context);
        var showRepository = new ShowRepository(context);
        var actorRepository = new ActorRepository(context);
        var roleRepository = new RoleRepository(context);
        var castMemberRepository = new CastMemberRepository(context);

        var theatre = new Theatre(1, "Test Theatre", "Test Info");
        var theatre2 = new Theatre(2, "Test Theatre 2", "Test Info");
        await theatreRepository.AddAsync(theatre);
        await theatreRepository.AddAsync(theatre2);
        await theatreRepository.SaveChangesAsync();

        var musical = new Musical(1, "Test Musical", "Test Description", TimeSpan.FromHours(2),
                      AgeRestriction.EighteenPlus, 1);
        var musical2 = new Musical(2, "Test Musical 2", "Test Description", TimeSpan.FromHours(2),
                      AgeRestriction.EighteenPlus, 2);
        await musicalRepository.AddAsync(musical);
        await musicalRepository.AddAsync(musical2);
        await musicalRepository.SaveChangesAsync();

        var show1 = new Show(1, DateTime.UtcNow.AddDays(7), 1);
        var show2 = new Show(2, DateTime.UtcNow.AddDays(8), 2);
        await showRepository.AddAsync(show1);
        await showRepository.AddAsync(show2);
        await showRepository.SaveChangesAsync();

        var actor1 = new Actor(1, "Test Actor", VoiceType.Tenor, Gender.Male, DateTime.Now.AddYears(-30), 175, 70, "Bio");
        var actor2 = new Actor(2, "Test Actor 2", VoiceType.Bass, Gender.Male, DateTime.Now.AddYears(-35), 180, 80, "Bio");
        var actor3 = new Actor(3, "Test Actor 3", VoiceType.Soprano, Gender.Female, DateTime.Now.AddYears(-25), 165, 60, "Bio");
        await actorRepository.AddAsync(actor1);
        await actorRepository.AddAsync(actor2);
        await actorRepository.AddAsync(actor3);
        await actorRepository.SaveChangesAsync();

        var role1 = new Role(1, "Test Role", 1, RoleType.Main);
        var role2 = new Role(2, "Test Role 2", 2, RoleType.Supporting);
        await roleRepository.AddAsync(role1);
        await roleRepository.AddAsync(role2);
        await roleRepository.SaveChangesAsync();

        var showId = 1;
        var castMember1 = _fixture.CreateCastMember(showId: showId, actorId: 1, roleId: 1);
        var castMember2 = _fixture.CreateCastMember(showId: showId, actorId: 2, roleId: 1);
        var castMember3 = _fixture.CreateCastMember(showId: 2, actorId: 3, roleId: 2);

        await castMemberRepository.AddAsync(castMember1);
        await castMemberRepository.AddAsync(castMember2);
        await castMemberRepository.AddAsync(castMember3);
        await castMemberRepository.SaveChangesAsync();



        var result = await castMemberRepository.GetByShowIdAsync(showId);


        result.Should().HaveCount(2);
        result.All(cm => cm.ShowId == showId).Should().BeTrue();
    }

    [Fact]
    [AllureFeature("GetByShowIdAsync")]
    [AllureStory("Negative case - no cast members for show")]
    public async Task GetByShowIdAsync_NoCastMembersForShow_ReturnsEmpty()
    {

        var repository = GetInMemoryRepository();
        var castMember = _fixture.CreateCastMember(showId: 1);

        await repository.AddAsync(castMember);
        await repository.SaveChangesAsync();



        var result = await repository.GetByShowIdAsync(999);


        result.Should().BeEmpty();
    }

    [Fact]
    [AllureFeature("GetByActorIdAsync")]
    [AllureStory("Positive case - returns cast members by actor")]
    public async Task GetByActorIdAsync_ValidActorId_ReturnsCastMembers()
    {
        var contextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new AppDbContext(contextOptions);

        var theatreRepository = new TheatreRepository(context);
        var musicalRepository = new MusicalRepository(context);
        var showRepository = new ShowRepository(context);
        var actorRepository = new ActorRepository(context);
        var roleRepository = new RoleRepository(context);
        var castMemberRepository = new CastMemberRepository(context);

        var theatre = new Theatre(1, "Test Theatre", "Test Info");
        var theatre2 = new Theatre(2, "Test Theatre 2", "Test Info");
        await theatreRepository.AddAsync(theatre);
        await theatreRepository.AddAsync(theatre2);
        await theatreRepository.SaveChangesAsync();

        var musical = new Musical(1, "Test Musical", "Test Description", TimeSpan.FromHours(2),
                      AgeRestriction.EighteenPlus, 1);
        var musical2 = new Musical(2, "Test Musical 2", "Test Description", TimeSpan.FromHours(2),
                      AgeRestriction.EighteenPlus, 2);
        await musicalRepository.AddAsync(musical);
        await musicalRepository.AddAsync(musical2);
        await musicalRepository.SaveChangesAsync();

        var show1 = new Show(1, DateTime.UtcNow.AddDays(7), 1);
        var show2 = new Show(2, DateTime.UtcNow.AddDays(8), 2);
        await showRepository.AddAsync(show1);
        await showRepository.AddAsync(show2);
        await showRepository.SaveChangesAsync();

        var actor1 = new Actor(1, "Test Actor", VoiceType.Tenor, Gender.Male, DateTime.Now.AddYears(-30), 175, 70, "Bio");
        var actor2 = new Actor(2, "Test Actor 2", VoiceType.Bass, Gender.Male, DateTime.Now.AddYears(-35), 180, 80, "Bio");
        var actor3 = new Actor(3, "Test Actor 3", VoiceType.Soprano, Gender.Female, DateTime.Now.AddYears(-25), 165, 60, "Bio");
        await actorRepository.AddAsync(actor1);
        await actorRepository.AddAsync(actor2);
        await actorRepository.AddAsync(actor3);
        await actorRepository.SaveChangesAsync();

        var role1 = new Role(1, "Test Role", 1, RoleType.Main);
        var role2 = new Role(2, "Test Role 2", 2, RoleType.Supporting);
        await roleRepository.AddAsync(role1);
        await roleRepository.AddAsync(role2);
        await roleRepository.SaveChangesAsync();

        var actorId = 1;
        var castMember1 = _fixture.CreateCastMember(actorId: actorId, showId: 1, roleId: 1);
        var castMember2 = _fixture.CreateCastMember(actorId: actorId, showId: 2, roleId: 2);
        var castMember3 = _fixture.CreateCastMember(actorId: 2, showId: 1, roleId: 1);

        await castMemberRepository.AddAsync(castMember1);
        await castMemberRepository.AddAsync(castMember2);
        await castMemberRepository.AddAsync(castMember3);
        await castMemberRepository.SaveChangesAsync();



        var result = await castMemberRepository.GetByActorIdAsync(actorId);


        result.Should().HaveCount(2);
        result.All(cm => cm.ActorId == actorId).Should().BeTrue();
    }

    [Fact]
    [AllureFeature("GetByActorIdAsync")]
    [AllureStory("Negative case - no cast members for actor")]
    public async Task GetByActorIdAsync_NoCastMembersForActor_ReturnsEmpty()
    {

        var repository = GetInMemoryRepository();
        var castMember = _fixture.CreateCastMember(actorId: 1);

        await repository.AddAsync(castMember);
        await repository.SaveChangesAsync();



        var result = await repository.GetByActorIdAsync(999);


        result.Should().BeEmpty();
    }

    [Fact]
    [AllureFeature("GetByShowAndRoleAsync")]
    [AllureStory("Positive case - returns cast member by show and role")]
    public async Task GetByShowAndRoleAsync_ValidData_ReturnsCastMember()
    {

        var repository = GetInMemoryRepository();
        var showId = 1;
        var roleId = 1;
        var castMember = _fixture.CreateCastMember(showId: showId, roleId: roleId);

        await repository.AddAsync(castMember);
        await repository.SaveChangesAsync();



        var result = await repository.GetByShowAndRoleAsync(showId, roleId);


        result.Should().NotBeNull();
        result.ShowId.Should().Be(showId);
        result.RoleId.Should().Be(roleId);
    }

    [Fact]
    [AllureFeature("GetByShowAndRoleAsync")]
    [AllureStory("Negative case - no cast member for show and role")]
    public async Task GetByShowAndRoleAsync_NoMatch_ReturnsNull()
    {

        var repository = GetInMemoryRepository();


        var result = await repository.GetByShowAndRoleAsync(999, 999);


        result.Should().BeNull();
    }
}