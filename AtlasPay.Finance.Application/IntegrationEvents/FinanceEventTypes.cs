namespace AtlasPay.Finance.Application.IntegrationEvents;

public static class FinanceEventTypes
{
    public const string TransferCompleted = "finance.transfer.completed.v1";
    public const string DepositCompleted = "finance.deposit.completed.v1";
    public const string WithdrawalCompleted = "finance.withdrawal.completed.v1";
    public const string TransferReversed = "finance.transfer.reversed.v1";
}