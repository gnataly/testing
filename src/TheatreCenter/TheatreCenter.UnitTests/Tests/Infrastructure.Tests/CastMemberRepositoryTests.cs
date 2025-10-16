using Allure.Xunit.Attributes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TheatreCenter.Data;
using TheatreCenter.Data.Repositories;
using TheatreCenter.Domain.Models;
using TheatreCenter.Tests.Fixtures;
using Xunit;

namespace TheatreCenter.Tests.Repositories;

[AllureSuite("Cast Member Repository Tests")]
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
        
        var repository = GetInMemoryRepository();
        var castMember = _fixture.CreateCastMember();

        await repository.AddAsync(castMember);
        

        
        var result = await repository.GetByIdAsync(castMember.Id);

        
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
        
        var repository = GetInMemoryRepository();
        var castMember1 = _fixture.CreateCastMember();
        var castMember2 = _fixture.CreateCastMember();

        await repository.AddAsync(castMember1);
        await repository.AddAsync(castMember2);
        

        
        var result = await repository.GetAllAsync();

        
        result.Should().HaveCount(2);
    }

    [Fact]
    [AllureFeature("GetAllAsync")]
    [AllureStory("Negative case - no cast members")]
    public async Task GetAllAsync_NoCastMembers_ReturnsEmpty()
    {
        
        var repository = GetInMemoryRepository();

        
        var result = await repository.GetAllAsync();

        
        result.Should().BeEmpty();
    }

    [Fact]
    [AllureFeature("AddAsync")]
    [AllureStory("Positive case - adds cast member")]
    public async Task AddAsync_ValidCastMember_AddsToDatabase()
    {
        
        var repository = GetInMemoryRepository();
        var castMember = _fixture.CreateCastMember();

        
        await repository.AddAsync(castMember);
        

        
        var result = await repository.GetByIdAsync(castMember.Id);
        result.Should().NotBeNull();
    }

    [Fact]
    [AllureFeature("UpdateAsync")]
    [AllureStory("Positive case - updates cast member")]
    public async Task UpdateAsync_ValidCastMember_UpdatesSuccessfully()
    {
        
        var repository = GetInMemoryRepository();
        var castMember = _fixture.CreateCastMember(comment: "Original comment");

        await repository.AddAsync(castMember);
        

        var existingCastMember = await repository.GetByIdAsync(castMember.Id);
        existingCastMember.Comment = "Updated comment";

        
        await repository.UpdateAsync(existingCastMember);

        
        var result = await repository.GetByIdAsync(castMember.Id);
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
        

        
        await repository.RemoveAsync(castMember);
        

        
        var result = await repository.GetByIdAsync(castMember.Id);
        result.Should().BeNull();
    }

    [Fact]
    [AllureFeature("GetByShowIdAsync")]
    [AllureStory("Positive case - returns cast members by show")]
    public async Task GetByShowIdAsync_ValidShowId_ReturnsCastMembers()
    {
        
        var repository = GetInMemoryRepository();
        var showId = 1;
        var castMember1 = _fixture.CreateCastMember(showId: showId);
        var castMember2 = _fixture.CreateCastMember(showId: showId);
        var castMember3 = _fixture.CreateCastMember(showId: 2);

        await repository.AddAsync(castMember1);
        await repository.AddAsync(castMember2);
        await repository.AddAsync(castMember3);
        

        
        var result = await repository.GetByShowIdAsync(showId);

        
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
        

        
        var result = await repository.GetByShowIdAsync(999);

        
        result.Should().BeEmpty();
    }

    [Fact]
    [AllureFeature("GetByActorIdAsync")]
    [AllureStory("Positive case - returns cast members by actor")]
    public async Task GetByActorIdAsync_ValidActorId_ReturnsCastMembers()
    {
        
        var repository = GetInMemoryRepository();
        var actorId = 1;
        var castMember1 = _fixture.CreateCastMember(actorId: actorId);
        var castMember2 = _fixture.CreateCastMember(actorId: actorId);
        var castMember3 = _fixture.CreateCastMember(actorId: 2);

        await repository.AddAsync(castMember1);
        await repository.AddAsync(castMember2);
        await repository.AddAsync(castMember3);
        

        
        var result = await repository.GetByActorIdAsync(actorId);

        
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