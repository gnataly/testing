using Microsoft.Extensions.Configuration;
using Npgsql;
using Xunit;

namespace TheatreCenter.UnitTests.Tests.Database;

public class DatabaseFixture : IAsyncLifetime
{
    private readonly string _baseConnectionString;
    private readonly string _databaseName;
    private readonly string _scriptsPath;

    public string ConnectionString { get; private set; }

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

        // Завершаем все активные соединения с тестовой БД
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

        // Разделяем скрипт по точкам с запятой, но игнорируем пустые команды
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
                Console.WriteLine($"Error executing command: {command}");
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }
    }

    public async Task ResetDatabaseAsync()
    {
        await ApplyScriptAsync("drop.sql");
        await ApplyScriptAsync("create.sql");
        await ApplyScriptAsync("init_data.sql");
    }
}