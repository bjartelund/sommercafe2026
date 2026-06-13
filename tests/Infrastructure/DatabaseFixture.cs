using CafeApp.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace CafeApp.Tests.Infrastructure;

public class DatabaseFixture : IDisposable
{
    private MsSqlContainer? _container;
    public AppDbContext DbContext { get; private set; } = null!;

    public DatabaseFixture()
    {
        InitializeAsync().GetAwaiter().GetResult();
    }

    private async Task InitializeAsync()
    {
        _container = new MsSqlBuilder()
            .WithPassword("Password@123")
            .Build();

        await _container.StartAsync();

        var connectionString = _container.GetConnectionString();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        DbContext = new AppDbContext(options);
        await DbContext.Database.MigrateAsync();
    }

    public void Dispose()
    {
        if (_container != null)
        {
            _container.StopAsync().GetAwaiter().GetResult();
            _container.DisposeAsync().GetAwaiter().GetResult();
        }
        DbContext?.Dispose();
    }
}
