using AtlasPay.Finance.Application.Abstractions.Persistence;
using AtlasPay.Finance.Domain.Transfers.Entities;
using Microsoft.EntityFrameworkCore;

namespace AtlasPay.Finance.Infrastructure.Persistence.Repositories;

public class TransferRepository : ITransferRepository
{
    private readonly FinanceDbContext _dbContext;

    public TransferRepository(FinanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        Transfer transfer,
        CancellationToken ct = default)
    {
        await _dbContext.Transfers.AddAsync(transfer, ct);
    }

    public async Task<Transfer?> GetByIdAsync(
        Guid transferId,
        CancellationToken ct = default)
    {
        return await _dbContext.Transfers
                    .SingleOrDefaultAsync(
                        x => x.Id == transferId,
                        ct);
    }

    public async Task<Transfer?> GetByRequestIdAsync(
        Guid requestId,
        Guid sourceCustomerId,
        CancellationToken ct = default)
    {
        return await _dbContext.Transfers
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                         x => x.RequestId == requestId &&
                              _dbContext.Accounts.Any(a =>
                                  a.Id == x.FromAccountId &&
                                  a.CustomerId == sourceCustomerId),
                         ct);
    }
}
