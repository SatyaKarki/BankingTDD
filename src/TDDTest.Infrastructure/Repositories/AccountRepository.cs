using Microsoft.EntityFrameworkCore;
using TDDTest.Domain.Entities;
using TDDTest.Domain.Interfaces;
using TDDTest.Infrastructure.Persistence;

namespace TDDTest.Infrastructure.Repositories;

public sealed class AccountRepository : IAccountRepository
{
    private readonly BankingDbContext _context;

    public AccountRepository(BankingDbContext context)
        => _context = context;

    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Accounts
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<Account?> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken = default)
        => await _context.Accounts
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber, cancellationToken);

    public async Task<IReadOnlyList<Account>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Accounts
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Account account, CancellationToken cancellationToken = default)
        => await _context.Accounts.AddAsync(account, cancellationToken);

    public void Update(Account account)
        => _context.Accounts.Update(account);

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Accounts.AnyAsync(a => a.Id == id, cancellationToken);
}
