using AtlasPay.Finance.Application.Abstractions.Persistence;
using AtlasPay.Finance.Application.Transfers.Exceptions;
using AtlasPay.Finance.Application.Transfers.IntegrationEvents;
using AtlasPay.Finance.Domain.Accounts.ValueObjects;
using AtlasPay.Finance.Domain.Ledger.Entities;
using AtlasPay.Finance.Domain.Ledger.ValueObjects;
using AtlasPay.Finance.Domain.Outbox.Entities;
using AtlasPay.Finance.Domain.Transfers.Entities;
using AtlasPay.Finance.Domain.Transfers.ValueObjects;
using System.Text.Json;

namespace AtlasPay.Finance.Application.Transfers.Commands.TransferMoney;

public sealed class TransferMoneyHandler(
    IUnitOfWork unitOfWork,
    IAccountLockRepository accountLockRepository,
    ITransferLedgerRepository transferLedgerRepository,
    ITransferRepository transferRepository,
    ILedgerTransactionRepository ledgerTransactionRepository,
    IOutboxRepository outboxRepository)
{
    public async Task<Guid> HandleAsync(
        TransferMoneyCommand command,
        CancellationToken ct = default)
    {
        if (command.RequestId == Guid.Empty)
            throw new ArgumentException(
                "RequestId cannot be empty.");

        // 1. Validate amount and currency.
        var amount = new Money(
            command.Amount,
            command.Currency);

        var transfer = Transfer.Create(
            command.RequestId,
            command.FromAccountId,
            command.ToAccountId,
            amount);

        // Fast path: the request may already be completed.
        var existing = await transferRepository
            .GetByRequestIdAsync(command.RequestId, ct);

        if (existing is not null)
            return ResolveExisting(existing, command);

        try
        {
            Guid resultId = Guid.Empty;

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

                // Check again after waiting for account locks.
                // Another transaction may have completed meanwhile.
                var alreadyProcessed = await transferRepository
                    .GetByRequestIdAsync(
                        command.RequestId,
                        token);
                if (alreadyProcessed is not null)
                {
                    resultId = ResolveExisting(
                        alreadyProcessed,
                        command);

                    return;
                }

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

                if (source.LedgerAccount.Type != LedgerAccountType.Liability ||
                    destination.LedgerAccount.Type != LedgerAccountType.Liability)
                {
                    throw new InvalidOperationException(
                        "Transfers require customer liability accounts.");
                }

                if (source.LedgerAccount.Currency != amount.Currency ||
                        destination.LedgerAccount.Currency != amount.Currency ||
                        source.Balance.Balance.Currency != amount.Currency ||
                        destination.Balance.Balance.Currency != amount.Currency)
                {
                    throw new InvalidOperationException(
                        "Ledger account currency mismatch.");
                }

                // 7. Create double-entry ledger transaction.
                var ledgerTransaction =
                    LedgerTransaction.Create(
                        transfer.Id,
                        LedgerTransactionType.Transfer);

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
                source.Balance.Debit(
                    amount,
                    source.LedgerAccount.Type);

                destination.Balance.Credit(
                    amount,
                    source.LedgerAccount.Type);

                // 10. Complete business transfer.
                transfer.Complete();

                // 11. Stage both aggregate roots.
                await transferRepository.AddAsync(
                    transfer, token);

                await ledgerTransactionRepository.AddAsync(
                    ledgerTransaction, token);

                // 12. Stage outbox messages.
                var eventId = Guid.NewGuid();
                var integrationEvent = new TransferCompletedV1(
                        EventId: eventId,
                        TransferId: transfer.Id,
                        FromAccountId: transfer.FromAccountId,
                        ToAccountId: transfer.ToAccountId,
                        Amount: transfer.Amount.Amount,
                        Currency: transfer.Amount.Currency.ToString(),
                        OccurredAt: DateTimeOffset.UtcNow);

                var payload = JsonSerializer.Serialize(
                        integrationEvent,
                        new JsonSerializerOptions(JsonSerializerDefaults.Web));

                var outboxMessage = OutboxMessage.Create(
                        eventId,
                        nameof(TransferCompletedV1),
                        payload);

                await outboxRepository.AddAsync(outboxMessage, ct);

                // ExecuteInTransactionAsync saves and commits.
                resultId = transfer.Id;
            },
            ct);

            return transfer.Id;
        }
        catch (DuplicateTransferRequestException)
        {
            // A competing transaction committed the same RequestId.
            // The transaction has already rolled back.
            // Re-read the winning result without tracking it.
            var winner = await transferRepository
                .GetByRequestIdAsync(command.RequestId, ct);

            if (winner is null)
                throw;

            return ResolveExisting(winner, command);
        }
    }

    private static Guid ResolveExisting(
       Transfer existing,
       TransferMoneyCommand command)
    {
        if (existing.FromAccountId != command.FromAccountId ||
            existing.ToAccountId != command.ToAccountId ||
            existing.Amount.Amount != command.Amount ||
            existing.Amount.Currency != command.Currency)
        {
            throw new TransferRequestConflictException();
        }

        if (existing.Status != TransferStatus.Completed)
        {
            throw new InvalidOperationException(
                $"Existing transfer is in {existing.Status} status.");
        }

        return existing.Id;
    }
}