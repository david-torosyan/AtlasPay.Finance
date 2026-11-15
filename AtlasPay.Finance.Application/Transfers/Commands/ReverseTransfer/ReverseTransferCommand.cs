namespace AtlasPay.Finance.Application.Transfers.Commands.ReverseTransfer;

public sealed record ReverseTransferCommand(
    Guid RequestId,
    Guid TransferId,
    string Reason);