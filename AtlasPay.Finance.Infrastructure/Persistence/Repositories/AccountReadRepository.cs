using AtlasPay.Finance.Application.Abstractions.Persistence;
using AtlasPay.Finance.Application.Accounts.Queries.GetAccount;
using AtlasPay.Finance.Application.Accounts.Queries.GetAccountBalance;
using AtlasPay.Finance.Domain.Accounts.Entities;
using AtlasPay.Finance.Domain.Ledger.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace AtlasPay.Finance.Infrastructure.Persistence.Repositories;

public class AccountReadRepository
    : IAccountReadRepository
{
    private readonly FinanceDbContext _dbContext;

    public AccountReadRepository(FinanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetAccountBalanceResult?> GetBalanceByIdAsync(
        Guid accountId,
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        return await (
            from account in _dbContext.Accounts.AsNoTracking()
            join ledgerAccount in _dbContext.LedgerAccounts.AsNoTracking()
                on account.Id equals ledgerAccount.AccountId
            join balance in _dbContext.AccountBalances.AsNoTracking()
                on ledgerAccount.Id equals balance.LedgerAccountId
            where account.Id == accountId
                && account.CustomerId == customerId
                && ledgerAccount.Type == LedgerAccountType.Liability
            select new GetAccountBalanceResult(
                account.Id,
                account.Currency,
                balance.Balance.Amount)
        ).SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<GetAccountResult?> GetByIdAsync(
        Guid accountId,
        Guid customerId,
        CancellationToken ct = default)
    {
        var account = await _dbContext.Accounts
            .AsNoTracking()
            .Where(a => a.Id == accountId &&
                        a.CustomerId == customerId)
            .Select(a => new
            {
                a.Id,
                a.CustomerId,
                a.Currency,
                a.Status
            })
            .SingleOrDefaultAsync(ct);

        if (account is null)
            return null;

        var balance = await (
            from ledgerAccount in _dbContext.LedgerAccounts
                .AsNoTracking()
            join accountBalance in _dbContext.AccountBalances
                 .AsNoTracking()
                 on ledgerAccount.Id equals accountBalance.LedgerAccountId
            where ledgerAccount.AccountId == accountId
              && ledgerAccount.Type == LedgerAccountType.Liability
            select (decimal?)accountBalance.Balance.Amount
        ).SingleOrDefaultAsync(ct);

        if (balance == null)
            throw new InvalidOperationException(
                    $"Primary liability balance not found for account {accountId}.");

        return new GetAccountResult(
           account.Id,
           account.CustomerId,
           account.Currency,
           account.Status,
           balance.Value);
    }

    public async Task<IReadOnlyList<GetAccountResult>> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken ct = default)
    {
        return await (
            from account in _dbContext.Accounts.AsNoTracking()
            join ledgerAccount in _dbContext.LedgerAccounts.AsNoTracking()
                on account.Id equals ledgerAccount.AccountId
            join balance in _dbContext.AccountBalances.AsNoTracking()
                on ledgerAccount.Id equals balance.LedgerAccountId
            where account.CustomerId == customerId
                && ledgerAccount.Type == LedgerAccountType.Liability
            orderby account.Currency, account.Id
            select new GetAccountResult(
                account.Id,
                account.CustomerId,
                account.Currency,
                account.Status,
                balance.Balance.Amount)
        ).ToListAsync(ct);
    }
}
