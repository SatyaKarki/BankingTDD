using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TDDTest.Domain.Interfaces;
using TDDTest.Infrastructure.Persistence;
using TDDTest.Infrastructure.Repositories;

namespace TDDTest.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers infrastructure services. Uses EF Core InMemory by default
    /// (ideal for development and integration tests). Pass a real connection
    /// string to swap to a persistent store without changing application code.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string? dbName = null)
    {
        var databaseName = dbName ?? "BankingDb";

        services.AddDbContext<BankingDbContext>(options =>
            options.UseInMemoryDatabase(databaseName));

        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
