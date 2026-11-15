using AtlasPay.Finance.Application.Abstractions.Persistence;
using AtlasPay.Finance.Domain.Deposits.Entities;
using Microsoft.EntityFrameworkCore;

namespace AtlasPay.Finance.Infrastructure.Persistence.Repositories;

public sealed class DepositRepository(
    FinanceDbContext dbContext) : IDepositRepository
{
    public async Task AddAsync(Deposit deposit, CancellationToken ct = default)
    {
        await dbContext.Deposits.AddAsync(deposit, ct);
    }

    public Task<Deposit?> GetByRequestIdAsync(Guid requestId, CancellationToken ct = default)
    {
        return dbContext.Deposits
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.RequestId == requestId,
                ct);
    }
}

