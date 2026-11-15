using AtlasPay.Finance.Domain.Transfers.Entities;

namespace AtlasPay.Finance.Application.Abstractions.Persistence;

public interface ITransferRepository
{
    Task AddAsync(
        Transfer transfer,
        CancellationToken ct = default);

    Task<Transfer?> GetByIdAsync(
        Guid transferId,
        CancellationToken ct = default);

    Task<Transfer?> GetByRequestIdAsync(
        Guid requestId,
        CancellationToken ct = default);
}