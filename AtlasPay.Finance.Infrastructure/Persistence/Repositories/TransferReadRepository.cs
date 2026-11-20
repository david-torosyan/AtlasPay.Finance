using AtlasPay.Finance.Application.Abstractions.Persistence;
using AtlasPay.Finance.Application.Transfers.Queries.GetAccountTransactions;
using AtlasPay.Finance.Application.Transfers.Queries.GetTransfer;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;

namespace AtlasPay.Finance.Infrastructure.Persistence.Repositories;

public class TransferReadRepository
    : ITransferReadRepository
{
    private readonly FinanceDbContext _dbContext;

    public TransferReadRepository(FinanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetAccountTransactionsResult> GetByAccountIdAsync(
        Guid accountId,
        Guid customerId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var ownsAccount = await _dbContext.Accounts
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == accountId &&
                     x.CustomerId == customerId,
                ct);

        if (!ownsAccount)
            throw new KeyNotFoundException("Account not found.");

        var query = _dbContext.Transfers
                      .AsNoTracking()
                      .Where(x =>
                           x.FromAccountId == accountId ||
                           x.ToAccountId == accountId
                      );

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new GetTransferResult(
                 x.Id,
                 x.RequestId,
                 x.FromAccountId,
                 x.ToAccountId,
                 x.Amount.Amount,
                 x.Amount.Currency,
                 x.Status,
                 x.CreatedAt,
                 x.CompletedAt))
            .ToListAsync(ct);

        return new GetAccountTransactionsResult(
           items,
           page,
           pageSize,
           totalCount);
    }

    public Task<GetTransferResult?> GetByIdAsync(
        Guid transferId,
        Guid customerId,
        CancellationToken ct = default)
    {
        return _dbContext.Transfers
                    .AsNoTracking()
                    .Where(t => t.Id == transferId &&
                        _dbContext.Accounts.Any(a =>
                            (a.Id == t.FromAccountId ||
                             a.Id == t.ToAccountId) &&
                            a.CustomerId == customerId))
                    .Select(x => new GetTransferResult(
                        x.Id,
                        x.RequestId,
                        x.FromAccountId,
                        x.ToAccountId,
                        x.Amount.Amount,
                        x.Amount.Currency,
                        x.Status,
                        x.CreatedAt,
                        x.CompletedAt))
                    .SingleOrDefaultAsync(ct);
    }
}
