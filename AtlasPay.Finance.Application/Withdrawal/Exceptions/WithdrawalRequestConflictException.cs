namespace AtlasPay.Finance.Application.Withdrawals.Exceptions;

public sealed class WithdrawalRequestConflictException : Exception
{
    public WithdrawalRequestConflictException()
        : base("RequestId was already used for a different withdrawal.")
    {
    }
}