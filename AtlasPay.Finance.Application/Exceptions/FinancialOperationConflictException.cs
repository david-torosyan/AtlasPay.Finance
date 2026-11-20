namespace AtlasPay.Finance.Application.Exceptions;

public sealed class FinancialOperationConflictException(string message)
    : Exception(message);
