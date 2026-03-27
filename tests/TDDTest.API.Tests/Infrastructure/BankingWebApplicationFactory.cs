using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore;
using TDDTest.Infrastructure.Persistence;

namespace TDDTest.API.Tests.Infrastructure;

/// <summary>
/// Custom WebApplicationFactory that replaces the EF Core InMemory database
/// with a new isolated instance per test run, preventing state bleed between tests.
/// </summary>
public sealed class BankingWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"TestBanking_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registration
            services.RemoveAll<DbContextOptions<BankingDbContext>>();
            services.RemoveAll<BankingDbContext>();

            // Register a fresh isolated InMemory DB per factory instance
            services.AddDbContext<BankingDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));
        });

        builder.UseEnvironment("Testing");
    }
}
