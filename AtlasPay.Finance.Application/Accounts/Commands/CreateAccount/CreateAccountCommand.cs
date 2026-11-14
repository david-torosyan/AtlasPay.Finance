using AtlasPay.Finance.Domain.Accounts.ValueObjects;

namespace AtlasPay.Finance.Application.Accounts.Commands.CreateAccount;

public sealed record CreateAccountCommand(
    Guid CustomerId,
    Currency Currency);