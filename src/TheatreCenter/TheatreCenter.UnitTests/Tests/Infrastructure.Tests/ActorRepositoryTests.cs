using Allure.Xunit.Attributes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TheatreCenter.Data;
using TheatreCenter.Data.Repositories;
using TheatreCenter.Domain.Enums;
using TheatreCenter.Domain.Models;
using TheatreCenter.Tests.Fixtures;
using TheatreCenter.UnitTests;
using Xunit;

namespace TheatreCenter.Tests.Repositories;

[AllureSuite("Actor Repository Tests")]
[Trait("Category", TestCategories.Unit)]
public class ActorRepositoryTests : IClassFixture<ActorFixture>
{
    private readonly ActorFixture _fixture;

    public ActorRepositoryTests(ActorFixture fixture)
    {
        _fixture = fixture;
    }

    private ActorRepository GetInMemoryRepository()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);
        return new ActorRepository(context);
    }

    [Fact]
    [AllureFeature("GetByIdAsync")]
    [AllureStory("Positive case - actor exists")]
    public async Task GetByIdAsync_ActorExists_ReturnsActor()
    {

        var repository = GetInMemoryRepository();
        var actor = _fixture.CreateActor();

        await repository.AddAsync(actor);
        await repository.SaveChangesAsync();



        var result = await repository.GetByIdAsync(actor.Id);


        result.Should().NotBeNull();
        result.Id.Should().Be(actor.Id);
        result.Name.Should().Be(actor.Name);
    }

    [Fact]
    [AllureFeature("GetByIdAsync")]
    [AllureStory("Negative case - actor not found")]
    public async Task GetByIdAsync_ActorNotExists_ReturnsNull()
    {

        var repository = GetInMemoryRepository();


        var result = await repository.GetByIdAsync(999);


        result.Should().BeNull();
    }

    [Fact]
    [AllureFeature("GetAllAsync")]
    [AllureStory("Positive case - returns all actors")]
    public async Task GetAllAsync_ActorsExist_ReturnsActors()
    {

        var repository = GetInMemoryRepository();
        var actor1 = _fixture.CreateActor(name: "Actor 1");
        var actor2 = _fixture.CreateActor(name: "Actor 2");

        await repository.AddAsync(actor1);
        await repository.AddAsync(actor2);
        await repository.SaveChangesAsync();



        var result = await repository.GetAllAsync(new ActorFilter());


        result.Should().HaveCount(2);
        result.Should().Contain(a => a.Name == "Actor 1");
        result.Should().Contain(a => a.Name == "Actor 2");
    }

    [Fact]
    [AllureFeature("GetAllAsync")]
    [AllureStory("Negative case - no actors")]
    public async Task GetAllAsync_NoActors_ReturnsEmpty()
    {

        var repository = GetInMemoryRepository();


        var result = await repository.GetAllAsync(new ActorFilter());


        result.Should().BeEmpty();
    }

    [Fact]
    [AllureFeature("AddAsync")]
    [AllureStory("Positive case - adds actor")]
    public async Task AddAsync_ValidActor_AddsToDatabase()
    {

        var repository = GetInMemoryRepository();
        var actor = _fixture.CreateActor();


        await repository.AddAsync(actor);
        await repository.SaveChangesAsync();



        var result = await repository.GetByIdAsync(actor.Id);
        result.Should().NotBeNull();
    }

    [Fact]
    [AllureFeature("UpdateAsync")]
    [AllureStory("Positive case - updates actor")]
    public async Task UpdateAsync_ValidActor_UpdatesSuccessfully()
    {

        var repository = GetInMemoryRepository();
        var actor = _fixture.CreateActor(name: "Original Name");

        await repository.AddAsync(actor);
        await repository.SaveChangesAsync();


        var existingActor = await repository.GetByIdAsync(actor.Id);
        existingActor.Name = "Updated Name";


        await repository.UpdateAsync(existingActor);
        await repository.SaveChangesAsync();


        var result = await repository.GetByIdAsync(actor.Id);
        result.Name.Should().Be("Updated Name");
    }

    [Fact]
    [AllureFeature("RemoveAsync")]
    [AllureStory("Positive case - removes actor")]
    public async Task RemoveAsync_ActorExists_RemovesActor()
    {

        var repository = GetInMemoryRepository();
        var actor = _fixture.CreateActor();

        await repository.AddAsync(actor);
        await repository.SaveChangesAsync();


        await repository.RemoveAsync(actor);
        await repository.SaveChangesAsync();


        var result = await repository.GetByIdAsync(actor.Id);
        result.Should().BeNull();
    }

    [Fact]
    [AllureFeature("GetAllAsync")]
    [AllureStory("Positive case - returns actors by voice type")]
    public async Task GetAllAsync_VoiceTypeFilter_ReturnsActors()
    {

        var repository = GetInMemoryRepository();
        var tenor1 = _fixture.CreateActor(voiceType: VoiceType.Tenor, name: "Tenor 1");
        var tenor2 = _fixture.CreateActor(voiceType: VoiceType.Tenor, name: "Tenor 2");
        var soprano = _fixture.CreateActor(voiceType: VoiceType.Soprano, name: "Soprano");

        await repository.AddAsync(tenor1);
        await repository.AddAsync(tenor2);
        await repository.AddAsync(soprano);
        await repository.SaveChangesAsync();



        var filter = new ActorFilter { VoiceType = VoiceType.Tenor };
        var result = await repository.GetAllAsync(filter);


        result.Should().HaveCount(2);
        result.Should().Contain(a => a.Name == "Tenor 1");
        result.Should().Contain(a => a.Name == "Tenor 2");
    }

    [Fact]
    [AllureFeature("GetAllAsync")]
    [AllureStory("Negative case - no actors with voice type")]
    public async Task GetAllAsync_NoActorsWithVoiceType_ReturnsEmpty()
    {

        var repository = GetInMemoryRepository();
        var soprano = _fixture.CreateActor(voiceType: VoiceType.Soprano);

        await repository.AddAsync(soprano);
        await repository.SaveChangesAsync();



        var filter = new ActorFilter { VoiceType = VoiceType.Tenor };
        var result = await repository.GetAllAsync(filter);


        result.Should().BeEmpty();
    }
}