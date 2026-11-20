using AtlasPay.Finance.Domain.Withdrawals.Entities;

namespace AtlasPay.Finance.Application.Abstractions.Persistence;

public interface IWithdrawalRepository
{
    Task AddAsync(
        Withdrawal withdrawal,
        CancellationToken ct = default);

    Task<Withdrawal?> GetByRequestIdAsync(
        Guid requestId,
        Guid customerId,
        CancellationToken ct = default);
}
