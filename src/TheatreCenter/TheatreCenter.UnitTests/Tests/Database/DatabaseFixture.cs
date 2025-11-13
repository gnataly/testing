using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using Xunit;
using Microsoft.EntityFrameworkCore;
using TheatreCenter.Data;
using Xunit.Abstractions;

namespace TheatreCenter.UnitTests.Tests.Database;

public class DatabaseFixture : IAsyncLifetime
{
    private readonly string _baseConnectionString;
    private readonly string _databaseName;
    private readonly string _scriptsPath;

    public string ConnectionString { get; private set; }
    public ITestOutputHelper Output { get; set; }

    public DatabaseFixture()
    {
        var tmpConnectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

        if (string.IsNullOrEmpty(tmpConnectionString))
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Test.json", optional: false, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            _baseConnectionString = config.GetConnectionString("TestDatabase")
                                    ?? throw new InvalidOperationException("Connection string 'TestDatabase' not found in appsettings.Test.json");
        }
        else
        {
            _baseConnectionString = tmpConnectionString;
        }

        _databaseName = $"theatre_test_{Guid.NewGuid():N}";

        var builder = new NpgsqlConnectionStringBuilder(_baseConnectionString)
        {
            Database = _databaseName
        };
        ConnectionString = builder.ToString();

        _scriptsPath = Path.Combine(Directory.GetCurrentDirectory(), "Tests", "Database", "schemas");
    }

    public async Task InitializeAsync()
    {
        await CreateDatabaseAsync();
        await ResetDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        await DropDatabaseAsync();
    }

    private async Task CreateDatabaseAsync()
    {
        var masterConnString = new NpgsqlConnectionStringBuilder(_baseConnectionString)
        {
            Database = "postgres"
        }.ToString();

        await using var conn = new NpgsqlConnection(masterConnString);
        await conn.OpenAsync();

        var createDbSql = $"CREATE DATABASE {_databaseName}";
        await using var cmd = new NpgsqlCommand(createDbSql, conn);
        await cmd.ExecuteNonQueryAsync();
    }

    private async Task DropDatabaseAsync()
    {
        var masterConnString = new NpgsqlConnectionStringBuilder(_baseConnectionString)
        {
            Database = "postgres"
        }.ToString();

        await using var conn = new NpgsqlConnection(masterConnString);
        await conn.OpenAsync();

        var terminateSql = $@"
            SELECT pg_terminate_backend(pid) 
            FROM pg_stat_activity 
            WHERE datname = '{_databaseName}' AND pid <> pg_backend_pid()";

        await using var terminateCmd = new NpgsqlCommand(terminateSql, conn);
        await terminateCmd.ExecuteNonQueryAsync();

        var dropDbSql = $"DROP DATABASE IF EXISTS {_databaseName}";
        await using var dropCmd = new NpgsqlCommand(dropDbSql, conn);
        await dropCmd.ExecuteNonQueryAsync();
    }

    private async Task ApplyScriptAsync(string scriptFile)
    {
        var fullPath = Path.Combine(_scriptsPath, scriptFile);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Script not found: {fullPath}");

        var sql = await File.ReadAllTextAsync(fullPath);

        await using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();

        var commands = sql.Split(';')
            .Select(cmd => cmd.Trim())
            .Where(cmd => !string.IsNullOrEmpty(cmd))
            .ToList();

        foreach (var command in commands)
        {
            try
            {
                await using var cmd = new NpgsqlCommand(command, conn);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Output?.WriteLine($"Error executing command: {command}");
                Output?.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }
    }

    public async Task ResetDatabaseAsync()
    {
        await ApplyScriptAsync("03-drop.sql");
        await ApplyScriptAsync("01-create.sql");
        await ApplyScriptAsync("02-init_data.sql");
    }

    public async Task<AppDbContext> CreateTransactionalContextAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        var context = new AppDbContext(options);
        await context.Database.BeginTransactionAsync();
        return context;
    }

    public async Task<(TRepository repository, Func<Task> commit, Func<Task> rollback)> CreateTransactionalRepositoryAsync<TRepository>()
        where TRepository : class
    {
        var context = await CreateTransactionalContextAsync();
        var repository = CreateRepository<TRepository>(context);

        return (repository,
            async () => { await context.Database.CommitTransactionAsync(); await context.DisposeAsync(); },
            async () => { await context.Database.RollbackTransactionAsync(); await context.DisposeAsync(); }
        );
    }

    public TRepository CreateRepository<TRepository>(AppDbContext context)
        where TRepository : class
    {
        var repositoryType = typeof(TRepository);

        // Создаем NullLogger для репозиториев, которые требуют ILogger
        if (repositoryType == typeof(TheatreCenter.Data.Repositories.ActorRepository))
        {
            var logger = NullLogger<TheatreCenter.Data.Repositories.ActorRepository>.Instance;
            return Activator.CreateInstance(repositoryType, context, logger) as TRepository;
        }

        // Для других репозиториев создаем с одним параметром
        return Activator.CreateInstance(repositoryType, context) as TRepository;
    }
}

[CollectionDefinition("Database collection")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture> { }