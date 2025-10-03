using Allure.Xunit.Attributes;
using FluentAssertions;
using Moq;
using TheatreCenter.Domain.Enums;
using TheatreCenter.Domain.Interfaces.Repositories;
using TheatreCenter.Domain.Models;
using TheatreCenter.Services.Services;
using TheatreCenter.Tests.Fixtures;
using Xunit;

namespace TheatreCenter.Tests.Services;

[AllureSuite("Role Service Tests")]
[AllureSubSuite("London Style (with Mocks)")]
public class RoleServiceMockTests : IClassFixture<RoleFixture>
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IMusicalRepository> _musicalRepositoryMock;
    private readonly RoleService _sut;
    private readonly RoleFixture _fixture;

    public RoleServiceMockTests(RoleFixture fixture)
    {
        _fixture = fixture;
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _musicalRepositoryMock = new Mock<IMusicalRepository>();
        _sut = new RoleService(_roleRepositoryMock.Object, _musicalRepositoryMock.Object);
    }

    [Fact]
    [AllureFeature("GetByIdAsync")]
    [AllureStory("Positive case - role exists")]
    public async Task GetByIdAsync_RoleExists_ReturnsRole()
    {
        // Arrange
        var roleId = 1;
        var expectedRole = _fixture.CreateRole(id: roleId);

        _roleRepositoryMock
            .Setup(repo => repo.GetByIdAsync(roleId))
            .ReturnsAsync(expectedRole);

        // Act
        var result = await _sut.GetByIdAsync(roleId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedRole);
        _roleRepositoryMock.Verify(repo => repo.GetByIdAsync(roleId), Times.Once);
    }

    [Fact]
    [AllureFeature("GetByIdAsync")]
    [AllureStory("Negative case - invalid ID")]
    public async Task GetByIdAsync_InvalidId_ThrowsArgumentException()
    {
        // Arrange
        var invalidId = 0;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.GetByIdAsync(invalidId));
        _roleRepositoryMock.Verify(repo => repo.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    [AllureFeature("GetByIdAsync")]
    [AllureStory("Negative case - role not found")]
    public async Task GetByIdAsync_RoleNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var roleId = 1;

        _roleRepositoryMock
            .Setup(repo => repo.GetByIdAsync(roleId))
            .ReturnsAsync((Role?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetByIdAsync(roleId));
        _roleRepositoryMock.Verify(repo => repo.GetByIdAsync(roleId), Times.Once);
    }

    [Fact]
    [AllureFeature("CreateAsync")]
    [AllureStory("Positive case - valid role")]
    public async Task CreateAsync_ValidRole_ReturnsCreatedRole()
    {
        // Arrange
        var role = _fixture.CreateRole();
        var musical = _fixture.CreateMusical(id: role.MusicalId);

        _musicalRepositoryMock
            .Setup(repo => repo.GetByIdAsync(role.MusicalId))
            .ReturnsAsync(musical);
        _roleRepositoryMock
            .Setup(repo => repo.AddAsync(role))
            .Returns(Task.CompletedTask);
        _roleRepositoryMock
            .Setup(repo => repo.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateAsync(role);

        // Assert
        result.Should().BeEquivalentTo(role);
        _musicalRepositoryMock.Verify(repo => repo.GetByIdAsync(role.MusicalId), Times.Once);
        _roleRepositoryMock.Verify(repo => repo.AddAsync(role), Times.Once);
        _roleRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    [AllureFeature("CreateAsync")]
    [AllureStory("Negative case - null role")]
    public async Task CreateAsync_NullRole_ThrowsArgumentNullException()
    {
        // Arrange
        Role role = null!;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.CreateAsync(role));
        _roleRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Role>()), Times.Never);
    }

    [Fact]
    [AllureFeature("CreateAsync")]
    [AllureStory("Negative case - empty name")]
    public async Task CreateAsync_EmptyName_ThrowsArgumentException()
    {
        // Arrange
        var role = _fixture.CreateRole();
        role.Name = "";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateAsync(role));
        _roleRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Role>()), Times.Never);
    }

    [Fact]
    [AllureFeature("CreateAsync")]
    [AllureStory("Negative case - musical not found")]
    public async Task CreateAsync_MusicalNotFound_ThrowsArgumentException()
    {
        // Arrange
        var role = _fixture.CreateRole();

        _musicalRepositoryMock
            .Setup(repo => repo.GetByIdAsync(role.MusicalId))
            .ReturnsAsync((Musical?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateAsync(role));
        _roleRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Role>()), Times.Never);
    }

    [Fact]
    [AllureFeature("DeleteAsync")]
    [AllureStory("Positive case - role deleted")]
    public async Task DeleteAsync_ValidRole_ReturnsTrue()
    {
        // Arrange
        var roleId = 1;
        var role = _fixture.CreateRole(id: roleId);
        role.ActorRoles = new List<ActorRole>(); // No assigned actors

        _roleRepositoryMock
            .Setup(repo => repo.GetByIdAsync(roleId))
            .ReturnsAsync(role);
        _roleRepositoryMock
            .Setup(repo => repo.RemoveAsync(role))
            .Returns(Task.CompletedTask);
        _roleRepositoryMock
            .Setup(repo => repo.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.DeleteAsync(roleId);

        // Assert
        result.Should().BeTrue();
        _roleRepositoryMock.Verify(repo => repo.GetByIdAsync(roleId), Times.Once);
        _roleRepositoryMock.Verify(repo => repo.RemoveAsync(role), Times.Once);
        _roleRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    [AllureFeature("DeleteAsync")]
    [AllureStory("Negative case - role with assigned actors")]
    public async Task DeleteAsync_RoleWithActors_ThrowsInvalidOperationException()
    {
        // Arrange
        var roleId = 1;
        var role = _fixture.CreateRole(id: roleId);
        role.ActorRoles = new List<ActorRole> { new ActorRole(1, roleId) }; // Has assigned actors

        _roleRepositoryMock
            .Setup(repo => repo.GetByIdAsync(roleId))
            .ReturnsAsync(role);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.DeleteAsync(roleId));
        _roleRepositoryMock.Verify(repo => repo.RemoveAsync(It.IsAny<Role>()), Times.Never);
    }

    [Fact]
    [AllureFeature("GetByMusicalIdAsync")]
    [AllureStory("Positive case - roles found")]
    public async Task GetByMusicalIdAsync_ValidId_ReturnsRoles()
    {
        // Arrange
        var musicalId = 1;
        var roles = new List<Role>
        {
            _fixture.CreateRole(musicalId: musicalId),
            _fixture.CreateRole(musicalId: musicalId)
        };

        _roleRepositoryMock
            .Setup(repo => repo.GetByMusicalIdAsync(musicalId))
            .ReturnsAsync(roles);

        // Act
        var result = await _sut.GetByMusicalIdAsync(musicalId);

        // Assert
        result.Should().HaveCount(2);
        _roleRepositoryMock.Verify(repo => repo.GetByMusicalIdAsync(musicalId), Times.Once);
    }

    [Fact]
    [AllureFeature("GetByMusicalIdAsync")]
    [AllureStory("Negative case - invalid musical ID")]
    public async Task GetByMusicalIdAsync_InvalidId_ThrowsArgumentException()
    {
        // Arrange
        var invalidId = 0;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.GetByMusicalIdAsync(invalidId));
        _roleRepositoryMock.Verify(repo => repo.GetByMusicalIdAsync(It.IsAny<int>()), Times.Never);
    }
}