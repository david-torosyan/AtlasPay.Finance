namespace AtlasPay.Finance.Application.Deposits.Exceptions;

public sealed class DuplicateDepositRequestException : Exception;

public sealed class DepositRequestConflictException : Exception
{
    public DepositRequestConflictException()
        : base("RequestId was already used for a different deposit.")
    {
    }
}