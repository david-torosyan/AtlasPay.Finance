using AtlasPay.Finance.Domain.Deposits.Entities;

namespace AtlasPay.Finance.Application.Abstractions.Persistence;

public interface IDepositRepository
{
    Task AddAsync(
        Deposit deposit,
        CancellationToken ct = default);

    Task<Deposit?> GetByRequestIdAsync(
        Guid requestId,
        CancellationToken ct = default);
}
