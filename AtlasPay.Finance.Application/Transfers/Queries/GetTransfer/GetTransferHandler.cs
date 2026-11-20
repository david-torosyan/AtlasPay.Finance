using AtlasPay.Finance.Application.Abstractions.Identity;
using AtlasPay.Finance.Application.Abstractions.Persistence;

namespace AtlasPay.Finance.Application.Transfers.Queries.GetTransfer;

public sealed class GetTransferHandler(
    ITransferReadRepository repository,
    ICurrentCustomer currentCustomer)
{
    public Task<GetTransferResult?> HandleAsync(
        GetTransferQuery query,
        CancellationToken ct = default)
    {
        if (query.TransferId == Guid.Empty)
            throw new ArgumentException(
                "Transfer ID cannot be empty.");

        return repository.GetByIdAsync(
            query.TransferId,
            currentCustomer.CustomerId,
            ct);
    }
}
