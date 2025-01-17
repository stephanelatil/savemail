using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Backend.Tests.ServicesTest;


/// <summary>
/// Base class for PostgreSQL TestContainer-based tests.
/// Inherit from this class in your test classes to share a single container instance
/// across all tests within that class.
/// </summary>
public class PostgresTestcontainerBase : IAsyncLifetime
{
    // Container instance shared across all tests in the derived test class
    protected readonly PostgreSqlContainer _dbContainer;
    protected DbContextOptions<ApplicationDBContext> _dbContextOptions;

    // Constructor is called once per test class
    public PostgresTestcontainerBase()
    {
        this._dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithPassword("postgres")
            .WithUsername("postgres")
            .WithDatabase("testdb")
            .Build();
        //tmp empty options to remove warning. Will be overwritten when container started
        this._dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>().Options;
    }

    // Called automatically by xUnit before any tests in the class run
    public async Task InitializeAsync()
    {
        await this._dbContainer.StartAsync();
        
        this._dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
            .UseNpgsql(this._dbContainer.GetConnectionString())
            .Options;

        // Create database and run migrations
        await using var context = new ApplicationDBContext(this._dbContextOptions);
        await context.Database.MigrateAsync();
    }

    // Called automatically by xUnit after all tests in the class complete
    public async Task DisposeAsync()
    {
        await this._dbContainer.DisposeAsync();
    }

    // Call this at the start of each test to get a fresh context with transaction
    protected async Task<ApplicationDBContext> CreateContextAsync()
    {
        var context = new ApplicationDBContext(this._dbContextOptions);
        await context.Database.BeginTransactionAsync();
        return context;
    }

    // Add this method to clean the database between tests if needed
    protected async Task CleanDatabaseAsync()
    {
        await using var context = new ApplicationDBContext(this._dbContextOptions);
        var tables = context.Model.GetEntityTypes()
            .Select(t => t.GetTableName())
            .Where(t => t != null)
            .ToList();

        foreach (var table in tables)
        {
            if (table is null)
                continue;
            await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"@table\" RESTART IDENTITY CASCADE;", table);
        }
    }
}