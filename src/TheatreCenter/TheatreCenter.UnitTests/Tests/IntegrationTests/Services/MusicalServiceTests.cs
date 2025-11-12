using TheatreCenter.Data;
using TheatreCenter.Domain.Models;
using TheatreCenter.Domain.Enums;
using TheatreCenter.Services.Services;
using TheatreCenter.Tests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TheatreCenter.Data.Repositories;
using TheatreCenter.UnitTests.Tests.Database;
using TheatreCenter.UnitTests;
using Xunit;

namespace TheatreCenter.Tests.IntegrationTests.Services;

[Collection("Database collection")]
[Trait("Category", TestCategories.Integration)]
public class MusicalServiceIt(DatabaseFixture db) : IClassFixture<DatabaseFixture>
{
    private readonly MusicalFixture _musicalFixture = new MusicalFixture();
    private readonly TheatreFixture _theatreFixture = new TheatreFixture();

    private MusicalService CreateService()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;

        var context = new AppDbContext(options);
        var musicalRepository = new MusicalRepository(context);
        var theatreRepository = new TheatreRepository(context);
        return new MusicalService(musicalRepository, theatreRepository);
    }

    [Fact]
    public async Task Musical_FullCycle_WithFixtures()
    {
        //using (StreamWriter writer = new StreamWriter(@"C:\Users\gnata\OneDrive\Документы\log.txt"))
        //{
            var service = CreateService();

        // Создаем театр для мюзикла
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;

        var theatre = _theatreFixture.CreateTheatre(name: "Test Theatre");

        await using (var context = new AppDbContext(options))
        {
            await context.Theatres.AddAsync(theatre);
            await context.SaveChangesAsync();
        }
            //writer.WriteLine(theatre.Id);
        
        // Используем фикстуру для генерации мюзикла
        var testMusical = _musicalFixture.CreateMusical(
            title: "Test Musical",
            description: "Test Description",
            duration: TimeSpan.FromHours(2),
            ageRestriction: AgeRestriction.TwelvePlus,
            theatreId: theatre.Id
        );
        //using (StreamWriter writer = new StreamWriter(@"C:\Users\gnata\OneDrive\Документы\log.txt"))
        //{
            // Act 1 — создание мюзикла
            var createdMusical = await service.CreateMusicalAsync(testMusical);

            // Assert 1 — проверка создания
            createdMusical.Should().NotBeNull();
            createdMusical.Id.Should().BeGreaterThan(0);
            createdMusical.Title.Should().Be(testMusical.Title);

            // Act 2 — получение мюзикла по ID
            var retrievedMusical = await service.GetMusicalByIdAsync(createdMusical.Id);
            retrievedMusical.Should().NotBeNull();
            retrievedMusical.Title.Should().Be(testMusical.Title);

            //writer.WriteLine($"!!!!!!!!Musical: {retrievedMusical.Title}, Musicalid: {retrievedMusical.Id}, AgeRestriction: {retrievedMusical.AgeRestriction}");


            // Act 3 — обновление мюзикла
            var updatedMusical = _musicalFixture.CreateMusical(
                id: createdMusical.Id,
                title: "Updated Musical Title",
                description: "Updated Description",
                ageRestriction: createdMusical.AgeRestriction,
                theatreId: theatre.Id
            );

            //writer.WriteLine($"upMusical: {updatedMusical.Title}, Musicalid: {updatedMusical.Id}, AgeRestriction: {updatedMusical.AgeRestriction}");


            var updateResult = await service.UpdateMusicalAsync(updatedMusical);
            updateResult.Should().NotBeNull();
            updateResult.Title.Should().Be("Updated Musical Title");

            // Act 4 — получение мюзиклов по театру
            var musicalsByTheatre = await service.GetMusicalsByTheatreAsync(theatre.Id);
            musicalsByTheatre.Should().Contain(m => m.Id == createdMusical.Id);

            // Act 5 — получение мюзиклов по возрастному ограничению
            var musicalsByAge = await service.GetMusicalsByAgeRestrictionAsync(AgeRestriction.TwelvePlus);
            //musicalsByAge.Should().Contain(m => m.Id == createdMusical.Id);

            // Act 6 — получение всех мюзиклов
            var allMusicals = await service.GetAllMusicalsAsync();
            //allMusicals.Should().Contain(m => m.Id == createdMusical.Id);
            // Создаем или перезаписываем файл

            //writer.WriteLine($"\ncreatedid = {createdMusical.Id}");
            //foreach (var musical in allMusicals)
            //{
            //    writer.WriteLine($"Musical: {musical.Title}, Musicalid: {musical.Id}, AgeRestriction: {musical.AgeRestriction}");
            //}


            // Act 7 — удаление мюзикла
            var deleteResult = await service.DeleteMusicalAsync(createdMusical.Id);
            deleteResult.Should().BeTrue();

            //// Assert 7 — проверка удаления
            //var deletedMusical = await service.GetMusicalByIdAsync(createdMusical.Id);
            //deletedMusical.Should().BeNull();
        //}
    }
}