using AtlasPay.Finance.Domain.Transfers.Entities;

namespace AtlasPay.Finance.Application.Abstractions.Persistence;

public interface ITransferReversalRepository
{
    Task AddAsync(
        TransferReversal reversal,
        CancellationToken ct = default);

    Task<TransferReversal?> GetByRequestIdAsync(
        Guid requestId,
        CancellationToken ct = default);

    Task<TransferReversal?> GetByOriginalTransferIdAsync(
        Guid transferId,
        CancellationToken ct = default);
}