using TDDTest.Domain.Interfaces;
using TDDTest.Infrastructure.Persistence;

namespace TDDTest.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly BankingDbContext _context;

    public UnitOfWork(BankingDbContext context)
        => _context = context;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
