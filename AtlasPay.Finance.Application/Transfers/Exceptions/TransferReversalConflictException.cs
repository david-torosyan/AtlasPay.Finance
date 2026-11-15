namespace AtlasPay.Finance.Application.Transfers.Exceptions;

public sealed class TransferReversalConflictException(string message)
    : Exception(message);