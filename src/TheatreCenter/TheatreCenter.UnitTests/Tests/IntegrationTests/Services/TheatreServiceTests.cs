using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheatreCenter.Data.Repositories;
using TheatreCenter.Data;
using TheatreCenter.Services.Services;
using TheatreCenter.Tests.Fixtures;
using TheatreCenter.UnitTests.Tests.Database;
using Xunit;
using FluentAssertions;

namespace TheatreCenter.UnitTests.Tests.IntegrationTests.Services;

[Collection("Database collection")]
[Trait("Category", TestCategories.Integration)]
public class TheatreServiceIt(DatabaseFixture db) : IClassFixture<DatabaseFixture>
{
    private readonly TheatreFixture _theatreFixture = new TheatreFixture();

    private TheatreService CreateService()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;

        var context = new AppDbContext(options);
        var repository = new TheatreRepository(context);
        return new TheatreService(repository);
    }

    [Fact]
    public async Task Theatre_FullCycle_WithFixtures()
    {
        using (StreamWriter writer = new StreamWriter(@"C:\Users\gnata\OneDrive\Документы\log.txt"))
        {
            var service = CreateService();

            // Используем фикстуру для генерации театра
            var testTheatre = _theatreFixture.CreateTheatre(
                name: "Test Theatre",
                addInfo: "Test additional info"
            );

            writer.WriteLine(testTheatre.Id);

            // Act 1 — создание театра
            var createdTheatre = await service.CreateTheatreAsync(testTheatre);

            // Assert 1 — проверка создания
            createdTheatre.Should().NotBeNull();
            createdTheatre.Id.Should().BeGreaterThan(0);
            createdTheatre.Name.Should().Be(testTheatre.Name);

            // Act 2 — получение театра по ID
            var retrievedTheatre = await service.GetTheatreByIdAsync(createdTheatre.Id);
            retrievedTheatre.Should().NotBeNull();
            retrievedTheatre.Name.Should().Be(testTheatre.Name);

            // Act 3 — обновление театра
            var updatedTheatre = _theatreFixture.CreateTheatre(
                id: createdTheatre.Id,
                name: "Updated Theatre Name",
                addInfo: "Updated additional info"
            );

            var updateResult = await service.UpdateTheatreAsync(updatedTheatre);
            updateResult.Should().NotBeNull();
            updateResult.Name.Should().Be("Updated Theatre Name");

            // Act 4 — получение всех театров
            var allTheatres = await service.GetAllTheatresAsync();
            allTheatres.Should().Contain(t => t.Id == createdTheatre.Id);

            // Act 5 — удаление театра
            var deleteResult = await service.DeleteTheatreAsync(createdTheatre.Id);
            deleteResult.Should().BeTrue();

            //// Assert 5 — проверка удаления
            //var deletedTheatre = await service.GetTheatreByIdAsync(createdTheatre.Id);
            //deletedTheatre.Should().BeNull();
        }
    }
}