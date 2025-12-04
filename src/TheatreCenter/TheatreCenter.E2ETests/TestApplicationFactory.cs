using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.TestHost;
using TheatreCenter.Backend.WebAPI;
using TheatreCenter.Data;

namespace TheatreCenter.E2ETests;

public class TestApplicationFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    protected override IHost CreateHost(IHostBuilder _)
    {
        var app = Program.BuildWebApplication(Array.Empty<string>(), ConfigureTestBuilder, AfterBuild);
        app.Start();
        return app;
    }

    private void ConfigureTestBuilder(WebApplicationBuilder builder)
    {
        builder.Environment.EnvironmentName = "IntegrationTests";
        builder.WebHost.UseTestServer();

        var mailUser = Environment.GetEnvironmentVariable("E2E_MAIL_USERNAME") ?? "testnataly@mail.ru";
        var mailPassword = Environment.GetEnvironmentVariable("E2E_MAIL_PASSWORD");
        var smtpHost = Environment.GetEnvironmentVariable("E2E_MAIL_SMTP_HOST") ?? "smtp.mail.ru";
        var smtpPort = Environment.GetEnvironmentVariable("E2E_MAIL_SMTP_PORT") ?? "587";

        var overrides = new Dictionary<string, string?>
        {
            ["Email:Username"] = mailUser,
            ["Email:From"] = mailUser,
            ["Email:Password"] = mailPassword,
            ["Email:SmtpHost"] = smtpHost,
            ["Email:SmtpPort"] = smtpPort,
            ["Email:DisableDelivery"] = "false"
        }.Where(kv => kv.Value != null).ToDictionary(kv => kv.Key, kv => kv.Value);

        builder.Configuration.AddInMemoryCollection(overrides!);

        var descriptor = builder.Services.SingleOrDefault(
            d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
        if (descriptor != null)
        {
            builder.Services.Remove(descriptor);
        }

        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        builder.Services.AddSingleton(_connection);

        builder.Services.AddDbContext<AppDbContext>((provider, options) =>
        {
            var connection = provider.GetRequiredService<SqliteConnection>();
            options.UseSqlite(connection);
        });
    }

    private void AfterBuild(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection?.Dispose();
        }
    }
}
