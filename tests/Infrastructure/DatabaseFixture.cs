using CafeApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;

namespace CafeApp.Tests.Infrastructure;

public class DatabaseFixture : IDisposable
{
    private static readonly SemaphoreSlim InitializationLock = new(1, 1);
    private static MsSqlContainer? _sharedContainer;
    private static bool _cleanupRegistered;

    private readonly string _databaseName = $"CafeAppTests_{Guid.NewGuid():N}";
    public AppDbContext DbContext { get; private set; } = null!;

    public DatabaseFixture()
    {
        InitializeAsync().GetAwaiter().GetResult();
    }

    private async Task InitializeAsync()
    {
        await EnsureContainerStartedAsync();

        var connectionStringBuilder = new SqlConnectionStringBuilder(_sharedContainer!.GetConnectionString())
        {
            InitialCatalog = _databaseName,
            TrustServerCertificate = true
        };

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionStringBuilder.ConnectionString)
            .Options;

        DbContext = new AppDbContext(options);
        await DbContext.Database.EnsureDeletedAsync();
        await DbContext.Database.EnsureCreatedAsync();
    }

    private static async Task EnsureContainerStartedAsync()
    {
        if (_sharedContainer != null)
            return;

        await InitializationLock.WaitAsync();
        try
        {
            if (_sharedContainer != null)
                return;

            _sharedContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
                .WithPassword("Password@123")
                .WithEnvironment("MSSQL_MEMORY_LIMIT_MB", "1536")
                .Build();

            await _sharedContainer.StartAsync();

            if (!_cleanupRegistered)
            {
                AppDomain.CurrentDomain.ProcessExit += (_, _) =>
                {
                    if (_sharedContainer == null)
                        return;

                    _sharedContainer.DisposeAsync().AsTask().GetAwaiter().GetResult();
                };

                _cleanupRegistered = true;
            }
        }
        finally
        {
            InitializationLock.Release();
        }
    }

    public void Dispose()
    {
        DbContext.Database.EnsureDeleted();
        DbContext.Dispose();
    }
}