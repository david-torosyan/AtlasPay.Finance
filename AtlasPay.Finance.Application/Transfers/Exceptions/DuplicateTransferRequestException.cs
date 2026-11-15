namespace AtlasPay.Finance.Application.Transfers.Exceptions;

public sealed class DuplicateTransferRequestException
    : Exception
{
    public DuplicateTransferRequestException(Exception innerException)
        : base(
            "A transfer with this RequestId already exists.",
            innerException)
    {
    }
}