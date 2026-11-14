using AtlasPay.Finance.Application.Abstractions.Persistence;
using AtlasPay.Finance.Domain.Accounts.Entities;
using AtlasPay.Finance.Domain.Ledger.Entities;
using AtlasPay.Finance.Domain.Ledger.ValueObjects;

namespace AtlasPay.Finance.Application.Accounts.Commands.CreateAccount;

public sealed class CreateAccountHandler
{
    private readonly IAccountRepository _accounts;
    private readonly ILedgerAccountRepository _ledgerAccounts;
    private readonly IAccountBalanceRepository _balances;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAccountHandler(
        IAccountRepository accounts,
        ILedgerAccountRepository ledgerAccounts,
        IAccountBalanceRepository balances,
        IUnitOfWork unitOfWork)
    {
        _accounts = accounts;
        _ledgerAccounts = ledgerAccounts;
        _balances = balances;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> HandleAsync(
        CreateAccountCommand command,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var account = Account.Create(
            command.CustomerId,
            command.Currency);

        var ledgerAccount = LedgerAccount.Create(
            account.Id,
            LedgerAccountType.Liability,
            account.Currency);

        var balance = AccountBalance.Create(
            ledgerAccount.Id,
            account.Currency);

        await _accounts.AddAsync(account, ct);
        await _ledgerAccounts.AddAsync(ledgerAccount, ct);
        await _balances.AddAsync(balance, ct);

        await _unitOfWork.SaveChangesAsync(ct);

        return account.Id;
    }
}
