using AtlasPay.Finance.Application.Abstractions.Persistence;
using AtlasPay.Finance.Domain.Accounts.ValueObjects;
using AtlasPay.Finance.Domain.Ledger.Entities;
using AtlasPay.Finance.Domain.Ledger.ValueObjects;
using AtlasPay.Finance.Domain.Transfers.Entities;

namespace AtlasPay.Finance.Application.Transfers.Commands.TransferMoney;

public sealed class TransferMoneyHandler(
    IUnitOfWork unitOfWork,
    IAccountLockRepository accountLockRepository,
    ITransferLedgerRepository transferLedgerRepository,
    ITransferRepository transferRepository,
    ILedgerTransactionRepository ledgerTransactionRepository)
{
    public async Task<Guid> HandleAsync(
        TransferMoneyCommand command,
        CancellationToken ct = default)
    {
        // 1. Validate amount and currency.
        var amount = new Money(
            command.Amount,
            command.Currency);

        var transfer = Transfer.Create(
            command.FromAccountId,
            command.ToAccountId,
            amount);

        // 2. Execute everything in one DB transaction.
        await unitOfWork.ExecuteInTransactionAsync(
            async token =>
            {
                // 3. Lock both accounts.
                var accounts =
                    await accountLockRepository.LockAccountsAsync(
                        command.FromAccountId,
                        command.ToAccountId,
                        token);

                var sourceAccount = accounts.Single(
                    x => x.Id == command.FromAccountId);

                var destinationAccount = accounts.Single(
                    x => x.Id == command.ToAccountId);

                // 4. Validate account statuses.
                if (sourceAccount.Status != AccountStatus.Active ||
                    destinationAccount.Status != AccountStatus.Active)
                {
                    throw new InvalidOperationException(
                        "Both accounts must be active.");
                }

                // 5. Validate currencies.
                if (sourceAccount.Currency != amount.Currency ||
                    destinationAccount.Currency != amount.Currency)
                {
                    throw new InvalidOperationException(
                        "Account currency mismatch.");
                }

                // 6. Resolve ledger accounts and balances.
                var source =
                    await transferLedgerRepository.GetByAccountIdAsync(
                        command.FromAccountId,
                        token);

                var destination =
                    await transferLedgerRepository.GetByAccountIdAsync(
                        command.ToAccountId,
                        token);

                if (source.LedgerAccount.Currency != amount.Currency ||
                    destination.LedgerAccount.Currency != amount.Currency)
                {
                    throw new InvalidOperationException(
                        "Ledger account currency mismatch.");
                }

                // 7. Create double-entry ledger transaction.
                var ledgerTransaction =
                    LedgerTransaction.Create(transfer.Id);

                ledgerTransaction.AddPosting(
                    source.LedgerAccount.Id,
                    PostingDirection.Debit,
                    amount);

                ledgerTransaction.AddPosting(
                    destination.LedgerAccount.Id,
                    PostingDirection.Credit,
                    amount);

                // 8. Validate and finalize postings.
                ledgerTransaction.Post();

                // 9. Update tracked balances.
                // Debit validates sufficient funds.
                source.Balance.Debit(amount);
                destination.Balance.Credit(amount);

                // 10. Complete business transfer.
                transfer.Complete();

                // 11. Stage both aggregate roots.
                await transferRepository.AddAsync(
                    transfer, token);

                await ledgerTransactionRepository.AddAsync(
                    ledgerTransaction, token);

                // ExecuteInTransactionAsync saves and commits.
            },
            ct);

        return transfer.Id;
    }
}