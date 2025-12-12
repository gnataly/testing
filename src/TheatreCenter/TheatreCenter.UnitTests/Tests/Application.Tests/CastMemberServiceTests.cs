using Allure.Xunit.Attributes;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TheatreCenter.Domain.Interfaces.Repositories;
using TheatreCenter.Domain.Models;
using TheatreCenter.Services.Services;
using TheatreCenter.Tests.Fixtures;
using TheatreCenter.UnitTests;
using Xunit;

namespace TheatreCenter.Tests.Services;

[AllureSuite("Cast Member Service Tests")]
[AllureSubSuite("London Style (with Mocks)")]
[Trait("Category", TestCategories.Unit)]
public class CastMemberServiceMockTests : IClassFixture<CastMemberFixture>
{
    private readonly Mock<ICastMemberRepository> _castMemberRepositoryMock;
    private readonly Mock<IShowRepository> _showRepositoryMock;
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IActorRepository> _actorRepositoryMock;
    private readonly Mock<ILogger<CastMemberService>> _loggerMock;
    private readonly CastMemberService _sut;
    private readonly CastMemberFixture _fixture;

    public CastMemberServiceMockTests(CastMemberFixture fixture)
    {
        _fixture = fixture;
        _castMemberRepositoryMock = new Mock<ICastMemberRepository>();
        _showRepositoryMock = new Mock<IShowRepository>();
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _actorRepositoryMock = new Mock<IActorRepository>();
        _loggerMock = new Mock<ILogger<CastMemberService>>();
        _sut = new CastMemberService(
            _castMemberRepositoryMock.Object,
            _showRepositoryMock.Object,
            _roleRepositoryMock.Object,
            _actorRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    [AllureFeature("GetByIdAsync")]
    [AllureStory("Positive case - cast member exists")]
    public async Task GetByIdAsync_CastMemberExists_ReturnsCastMember()
    {

        var castMemberId = 1;
        var expectedCastMember = _fixture.CreateCastMember(id: castMemberId);

        _castMemberRepositoryMock
            .Setup(repo => repo.GetByIdAsync(castMemberId))
            .ReturnsAsync(expectedCastMember);


        var result = await _sut.GetByIdAsync(castMemberId);


        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedCastMember);
        _castMemberRepositoryMock.Verify(repo => repo.GetByIdAsync(castMemberId), Times.Once);
    }

    [Fact]
    [AllureFeature("GetByIdAsync")]
    [AllureStory("Negative case - invalid ID")]
    public async Task GetByIdAsync_InvalidId_ThrowsArgumentException()
    {

        var invalidId = 0;


        await Assert.ThrowsAsync<ArgumentException>(() => _sut.GetByIdAsync(invalidId));
        _castMemberRepositoryMock.Verify(repo => repo.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    [AllureFeature("GetByIdAsync")]
    [AllureStory("Negative case - cast member not found")]
    public async Task GetByIdAsync_CastMemberNotFound_ThrowsKeyNotFoundException()
    {

        var castMemberId = 1;

        _castMemberRepositoryMock
            .Setup(repo => repo.GetByIdAsync(castMemberId))
            .ReturnsAsync((CastMember?)null);


        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetByIdAsync(castMemberId));
        _castMemberRepositoryMock.Verify(repo => repo.GetByIdAsync(castMemberId), Times.Once);
    }

    [Fact]
    [AllureFeature("CreateAsync")]
    [AllureStory("Positive case - valid cast member")]
    public async Task CreateAsync_ValidCastMember_ReturnsCreatedCastMember()
    {

        var castMember = _fixture.CreateCastMember();
        var show = _fixture.CreateShow(id: castMember.ShowId);
        var role = _fixture.CreateRole(id: castMember.RoleId);
        var actor = _fixture.CreateActor(id: castMember.ActorId);

        _showRepositoryMock
            .Setup(repo => repo.GetByIdAsync(castMember.ShowId))
            .ReturnsAsync(show);
        _roleRepositoryMock
            .Setup(repo => repo.GetByIdAsync(castMember.RoleId))
            .ReturnsAsync(role);
        _actorRepositoryMock
            .Setup(repo => repo.GetByIdAsync(castMember.ActorId))
            .ReturnsAsync(actor);
        _castMemberRepositoryMock
            .Setup(repo => repo.GetByShowAndRoleAsync(castMember.ShowId, castMember.RoleId))
            .ReturnsAsync((CastMember?)null);
        _castMemberRepositoryMock
            .Setup(repo => repo.AddAsync(castMember))
            .Returns(Task.CompletedTask);
        _castMemberRepositoryMock
            .Setup(repo => repo.SaveChangesAsync())
            .Returns(Task.CompletedTask);


        var result = await _sut.CreateAsync(castMember);


        result.Should().BeEquivalentTo(castMember);
        _showRepositoryMock.Verify(repo => repo.GetByIdAsync(castMember.ShowId), Times.Once);
        _roleRepositoryMock.Verify(repo => repo.GetByIdAsync(castMember.RoleId), Times.Once);
        _actorRepositoryMock.Verify(repo => repo.GetByIdAsync(castMember.ActorId), Times.Once);
        _castMemberRepositoryMock.Verify(repo => repo.GetByShowAndRoleAsync(castMember.ShowId, castMember.RoleId), Times.Once);
        _castMemberRepositoryMock.Verify(repo => repo.AddAsync(castMember), Times.Once);
        _castMemberRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    [AllureFeature("CreateAsync")]
    [AllureStory("Negative case - role already assigned")]
    public async Task CreateAsync_RoleAlreadyAssigned_ThrowsInvalidOperationException()
    {

        var castMember = _fixture.CreateCastMember();
        var show = _fixture.CreateShow(id: castMember.ShowId);
        var role = _fixture.CreateRole(id: castMember.RoleId);
        var actor = _fixture.CreateActor(id: castMember.ActorId);
        var existingAssignment = _fixture.CreateCastMember();

        _showRepositoryMock
            .Setup(repo => repo.GetByIdAsync(castMember.ShowId))
            .ReturnsAsync(show);
        _roleRepositoryMock
            .Setup(repo => repo.GetByIdAsync(castMember.RoleId))
            .ReturnsAsync(role);
        _actorRepositoryMock
            .Setup(repo => repo.GetByIdAsync(castMember.ActorId))
            .ReturnsAsync(actor);
        _castMemberRepositoryMock
            .Setup(repo => repo.GetByShowAndRoleAsync(castMember.ShowId, castMember.RoleId))
            .ReturnsAsync(existingAssignment);


        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateAsync(castMember));
        _castMemberRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<CastMember>()), Times.Never);
    }

    [Fact]
    [AllureFeature("GetByShowIdAsync")]
    [AllureStory("Positive case - cast members found")]
    public async Task GetByShowIdAsync_ValidId_ReturnsCastMembers()
    {

        var showId = 1;
        var castMembers = new List<CastMember>
        {
            _fixture.CreateCastMember(showId: showId),
            _fixture.CreateCastMember(showId: showId)
        };

        _castMemberRepositoryMock
            .Setup(repo => repo.GetByShowIdAsync(showId))
            .ReturnsAsync(castMembers);


        var result = await _sut.GetByShowIdAsync(showId);


        result.Should().HaveCount(2);
        _castMemberRepositoryMock.Verify(repo => repo.GetByShowIdAsync(showId), Times.Once);
    }

    [Fact]
    [AllureFeature("GetByShowIdAsync")]
    [AllureStory("Negative case - invalid show ID")]
    public async Task GetByShowIdAsync_InvalidId_ThrowsArgumentException()
    {

        var invalidId = 0;


        await Assert.ThrowsAsync<ArgumentException>(() => _sut.GetByShowIdAsync(invalidId));
        _castMemberRepositoryMock.Verify(repo => repo.GetByShowIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    [AllureFeature("GetByActorIdAsync")]
    [AllureStory("Positive case - cast members found")]
    public async Task GetByActorIdAsync_ValidId_ReturnsCastMembers()
    {

        var actorId = 1;
        var castMembers = new List<CastMember>
        {
            _fixture.CreateCastMember(actorId: actorId),
            _fixture.CreateCastMember(actorId: actorId)
        };

        _castMemberRepositoryMock
            .Setup(repo => repo.GetByActorIdAsync(actorId))
            .ReturnsAsync(castMembers);


        var result = await _sut.GetByActorIdAsync(actorId);


        result.Should().HaveCount(2);
        _castMemberRepositoryMock.Verify(repo => repo.GetByActorIdAsync(actorId), Times.Once);
    }

    [Fact]
    [AllureFeature("GetByActorIdAsync")]
    [AllureStory("Negative case - invalid actor ID")]
    public async Task GetByActorIdAsync_InvalidId_ThrowsArgumentException()
    {

        var invalidId = 0;


        await Assert.ThrowsAsync<ArgumentException>(() => _sut.GetByActorIdAsync(invalidId));
        _castMemberRepositoryMock.Verify(repo => repo.GetByActorIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    [AllureFeature("GetAllAsync")]
    [AllureStory("Positive case - cast members exist")]
    public async Task GetAllAsync_CastMembersExist_ReturnsCastMembers()
    {
        var filter = new CastMemberFilter();
        var castMembers = new List<CastMember>
        {
            _fixture.CreateCastMember(),
            _fixture.CreateCastMember()
        };

        _castMemberRepositoryMock
            .Setup(repo => repo.GetAllAsync(filter))
            .ReturnsAsync(castMembers);


        var result = await _sut.GetAllAsync(filter);


        result.Should().HaveCount(2);
        _castMemberRepositoryMock.Verify(repo => repo.GetAllAsync(filter), Times.Once);
    }
}
