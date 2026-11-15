using AtlasPay.Finance.Domain.Accounts.ValueObjects;

namespace AtlasPay.Finance.Application.Transfers.Commands.TransferMoney;

public sealed record TransferMoneyCommand(
    Guid RequestId,
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    Currency Currency);