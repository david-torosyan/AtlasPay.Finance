namespace AtlasPay.Finance.Application.Transfers.Exceptions;

public sealed class DuplicateTransferRequestException
    : Exception
{
    public DuplicateTransferRequestException()
        : base("A transfer with this RequestId already exists.")
    {
    }
}