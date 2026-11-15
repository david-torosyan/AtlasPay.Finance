namespace AtlasPay.Finance.Api.Contracts.Transfers;

public sealed record ReverseTransferRequest(
    Guid RequestId,
    string Reason);