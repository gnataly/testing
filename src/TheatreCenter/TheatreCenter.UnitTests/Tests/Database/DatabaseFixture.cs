using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using Xunit;
using Microsoft.EntityFrameworkCore;
using TheatreCenter.Data;
using Xunit.Abstractions;
using TheatreCenter.Data.Repositories;
using System.Diagnostics;

namespace TheatreCenter.UnitTests.Tests.Database;

public class DatabaseFixture : IAsyncLifetime
{
    private readonly string _baseConnectionString;
    private readonly string _databaseName;
    private readonly string _scriptsPath;
    private static readonly SemaphoreSlim _globalLock = new(1, 1);
    private static readonly SemaphoreSlim _truncateLock = new(1, 1);
    private static readonly object _initLock = new object();

    private static bool _isDatabaseInitialized = false;
    private static int _initializationInProgress = 0;

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
                                ?? throw new InvalidOperationException("Connection string 'TestDatabase' not found in appsettings.Tests.json");
        }
        else
        {
            _baseConnectionString = tmpConnectionString;
        }

        var builder = new NpgsqlConnectionStringBuilder(_baseConnectionString);
        var processId = Process.GetCurrentProcess().Id;
        _databaseName = $"test_db_{Guid.NewGuid():N}";
        builder.Database = _databaseName;
        ConnectionString = _baseConnectionString;

        _scriptsPath = Path.Combine(Directory.GetCurrentDirectory(), "Tests", "Database", "schemas");

    }


    public async Task InitializeAsync()
    {
        await _globalLock.WaitAsync();
        try
        {
            await ApplyScriptAsync("01-create.sql");
            await TruncateAllAsync();
        }
        finally
        {
            _globalLock.Release();
        }
    }

    //public async Task InitializeAsync()
    //{
    //    lock (_initLock)
    //    {
    //        if (_isDatabaseInitialized)
    //            return;

    //        CreateDatabaseAsync();
    //        ApplyScriptAsync("01-create.sql").Wait();
    //        ApplyScriptAsync("02-init_data.sql").Wait();

    //        _isDatabaseInitialized = true;
    //    }
    //}

    public async Task DisposeAsync()
    {
        //ApplyScriptAsync("03-drop.sql");
        //DropDatabaseAsync();
    }

    //public async Task InitializeAsync()
    //{
    //    // Если инициализация уже идет, ждем
    //    if (Interlocked.CompareExchange(ref _initializationInProgress, 1, 0) == 1)
    //    {
    //        await WaitForInitializationAsync(TimeSpan.FromSeconds(30));
    //        return;
    //    }

    //    await _globalLock.WaitAsync();
    //    try
    //    {
    //        if (!_isDatabaseInitialized)
    //        {
    //            await ApplyScriptAsync("01-create.sql");
    //            await ApplyScriptAsync("02-init_data.sql");
    //            _isDatabaseInitialized = true;
    //        }
    //    }
    //    finally
    //    {
    //        Interlocked.Exchange(ref _initializationInProgress, 0);
    //        _globalLock.Release();
    //    }
    //}

    //private async Task WaitForInitializationAsync(TimeSpan timeout)
    //{
    //    var startTime = DateTime.UtcNow;

    //    while (!_isDatabaseInitialized && (DateTime.UtcNow - startTime) < timeout)
    //    {
    //        await Task.Delay(100);
    //    }

    //    if (!_isDatabaseInitialized)
    //        throw new TimeoutException("Database initialization timeout");
    //}

    //public async Task InitializeAsync()
    //{
    //    await _globalLock.WaitAsync();
    //    try
    //    {
    //        if (!_isDatabaseInitialized)
    //        {
    //            await CreateDatabaseAsync();
    //            await ApplyScriptAsync("01-create.sql");
    //            await ApplyScriptAsync("02-init_data.sql");
    //            _isDatabaseInitialized = true;
    //        }
    //    }
    //    finally
    //    {
    //        _globalLock.Release();
    //    }
    //}

    //public async Task InitializeAsync()
    //{


    //CreateDatabaseAsync();
    //ApplyScriptAsync("01-create.sql");
    //ApplyScriptAsync("02-init_data.sql");
    //}

    //public async Task DisposeAsync()
    //{
    //ApplyScriptAsync("03-drop.sql");
    //}

    //private async Task CreateDatabaseAsync()
    //{
    //    var masterConnString = new NpgsqlConnectionStringBuilder(_baseConnectionString)
    //    {
    //        Database = "postgres"
    //    }.ToString();

    //    await using var conn = new NpgsqlConnection(masterConnString);
    //    await conn.OpenAsync();

    //    var createDbSql = $"CREATE DATABASE {_databaseName}";
    //    await using var cmd = new NpgsqlCommand(createDbSql, conn);
    //    await cmd.ExecuteNonQueryAsync();
    //}

    //private async Task DropDatabaseAsync()
    //{
    //    var masterConnString = new NpgsqlConnectionStringBuilder(_baseConnectionString)
    //    {
    //        Database = "postgres"
    //    }.ToString();

    //    await using var conn = new NpgsqlConnection(masterConnString);
    //    await conn.OpenAsync();

    //    var terminateSql = $@"
    //        SELECT pg_terminate_backend(pid) 
    //        FROM pg_stat_activity 
    //        WHERE datname = '{_databaseName}' AND pid <> pg_backend_pid()";

    //    await using var terminateCmd = new NpgsqlCommand(terminateSql, conn);
    //    await terminateCmd.ExecuteNonQueryAsync();

    //    var dropDbSql = $"DROP DATABASE IF EXISTS {_databaseName}";
    //    await using var dropCmd = new NpgsqlCommand(dropDbSql, conn);
    //    await dropCmd.ExecuteNonQueryAsync();
    //}

    //public async Task WaitForDatabaseReadyAsync(TimeSpan timeout)
    //{
    //    var startTime = DateTime.UtcNow;

    //    while ((DateTime.UtcNow - startTime) < timeout)
    //    {
    //        try
    //        {
    //            await using var conn = new NpgsqlConnection(ConnectionString);
    //            await conn.OpenAsync();

    //            var checkTableSql = "SELECT 1 FROM \"Accounts\" LIMIT 1";
    //            await using var cmd = new NpgsqlCommand(checkTableSql, conn);
    //            await cmd.ExecuteScalarAsync();

    //            return;
    //        }
    //        catch
    //        {
    //            await Task.Delay(100);
    //        }
    //    }

    //    throw new TimeoutException("Database not ready within timeout");
    //}




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

    private async Task TruncateAllAsync()
    {
        const string truncateSql = @"
TRUNCATE ""AccountActorFavorites"",
         ""AccountMusicalFavorites"",
         ""AccountTheatreFavorites"",
         ""CastMembers"",
         ""ActorRoles"",
         ""MusicalThemes"",
         ""Shows"",
         ""Roles"",
         ""Musicals"",
         ""Themes"",
         ""Actors"",
         ""Theatres"",
         ""Accounts""
         RESTART IDENTITY CASCADE;";

        await _truncateLock.WaitAsync();
        try
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(truncateSql, conn);
            await cmd.ExecuteNonQueryAsync();
        }
        finally
        {
            _truncateLock.Release();
        }
    }




    //public async Task<AppDbContext> CreateTransactionalContextAsync()
    //{
    //    var options = new DbContextOptionsBuilder<AppDbContext>()
    //        .UseNpgsql(ConnectionString)
    //        .Options;

    //    var context = new AppDbContext(options);
    //    await context.Database.BeginTransactionAsync();
    //    return context;
    //}

    public async Task<AppDbContext> CreateTransactionalContextAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        var context = new AppDbContext(options);
        await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        return context;
    }

    //public async Task<(TRepository repository, Func<Task> commit, Func<Task> rollback, AppDbContext context)> CreateTransactionalRepositoryAsync<TRepository>(AppDbContext context = null)
    //    where TRepository : class
    //{
    //    if (context == null)
    //        context = await CreateTransactionalContextAsync();

    //    var repository = CreateRepository<TRepository>(context);

    //    return (repository,
    //        async () => { await context.Database.CommitTransactionAsync(); await context.DisposeAsync(); },
    //        async () => { await context.Database.RollbackTransactionAsync(); await context.DisposeAsync(); },
    //        context
    //    );
    //}

    public TRepository CreateRepository<TRepository>(AppDbContext context)
        where TRepository : class
    {
        var repositoryType = typeof(TRepository);

        return Activator.CreateInstance(repositoryType, context) as TRepository;
    }
}

[CollectionDefinition("Database collection", DisableParallelization = true)]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture> { }
