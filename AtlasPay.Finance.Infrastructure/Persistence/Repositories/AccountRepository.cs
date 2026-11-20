using AtlasPay.Finance.Application.Abstractions.Persistence;
using AtlasPay.Finance.Domain.Accounts.Entities;
using Microsoft.EntityFrameworkCore;

namespace AtlasPay.Finance.Infrastructure.Persistence.Repositories;

public sealed class AccountRepository : IAccountRepository
{
    private readonly FinanceDbContext _dbContext;

    public AccountRepository(FinanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        Account account,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Accounts.AddAsync(
            account,
            cancellationToken);
    }

    public Task<Account?> GetByIdAsync(
        Guid accountId,
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Accounts
            .SingleOrDefaultAsync(
                x => x.Id == accountId &&
                     x.CustomerId == customerId,
                cancellationToken);
    }
}
