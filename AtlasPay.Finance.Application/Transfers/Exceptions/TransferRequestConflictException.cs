namespace AtlasPay.Finance.Application.Transfers.Exceptions;

public sealed class TransferRequestConflictException
    : Exception
{
    public TransferRequestConflictException()
        : base(
            "RequestId was already used for different transfer details.")
    {
    }
}