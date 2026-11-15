
public sealed class DuplicateWithdrawalRequestException : Exception
{
    public DuplicateWithdrawalRequestException()
        : base("RequestId was already used for a different withdrawal.")
    {
    }
}